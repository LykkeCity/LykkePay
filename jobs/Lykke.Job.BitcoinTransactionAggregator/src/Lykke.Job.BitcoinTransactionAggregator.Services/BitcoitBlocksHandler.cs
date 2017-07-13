using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.AzureRepositories;
using Lykke.Common.Entities.Wallets;
using Lykke.Core;
using Lykke.Job.BitcoinTransactionAggregator.Core;
using Lykke.Job.BitcoinTransactionAggregator.Core.Services;
using Lykke.Pay.Service.Wallets.Client;
using Lykke.Pay.Service.Wallets.Client.Models;
using NBitcoin;
using NBitcoin.RPC;

namespace Lykke.Job.BitcoinTransactionAggregator.Services
{
    // NOTE: This is job service class example
    public class BitcoitBlocksHandler : IBitcoitBlocksHandler
    {
        private readonly AppSettings.BitcoinTransactionAggregatorSettings _settings;
        private readonly IBitcoinAggRepository _bitcoinAggRepository;
        private readonly RPCClient _rpcClient;
        private readonly ILog _log;
        private readonly IPayWalletservice _payWalletService;
        private readonly IBitcoinBroadcast _bitcoinBroadcast;


        public BitcoitBlocksHandler(AppSettings.BitcoinTransactionAggregatorSettings settings, IBitcoinAggRepository bitcoinAggRepository,
            RPCClient rpcClient, IPayWalletservice payWalletService, IBitcoinBroadcast bitcoinBroadcast, ILog log)
        {
            _settings = settings;
            _bitcoinAggRepository = bitcoinAggRepository;
            _rpcClient = rpcClient;
            _log = log;
            _payWalletService = payWalletService;
            _bitcoinBroadcast = bitcoinBroadcast;
        }
        public async Task ProcessAsync()
        {
          

            //IBitcoinAggRepository bitcoinRepo =
            //    new BitcoinAggRepository(
            //        new AzureTableStorage<BitcoinAggEntity>(
            //            generalSettings.LykkePayJobBitcointHandle.Db.MerchantWalletConnectionString, "BitcoinAgg",
            //            null),
            //        new AzureTableStorage<BitcoinHeightEntity>(
            //            generalSettings.LykkePayJobBitcointHandle.Db.MerchantWalletConnectionString, "BitcoinHeight",
            //            null));
            //IMerchantWalletRepository merchantWalletRepository =
            //    new MerchantWalletRepository(new AzureTableStorage<MerchantWalletEntity>(
            //        generalSettings.LykkePayJobBitcointHandle.Db.MerchantWalletConnectionString, "MerchantWallets",
            //        null));
            //IMerchantWalletHistoryRepository merchantWalletHistoryRepository =
            //    new MerchantWalletHistoryRepository(new AzureTableStorage<MerchantWalletHistoryEntity>(
            //        generalSettings.LykkePayJobBitcointHandle.Db.MerchantWalletConnectionString,
            //        "MerchantWalletsHistory", null));

            //var client =
            //    new rpcClient(
            //        new NetworkCredential(generalSettings.LykkePayJobBitcointHandle.Rpc.UserName,
            //            generalSettings.LykkePayJobBitcointHandle.Rpc.Password),
            //        new Uri(generalSettings.LykkePayJobBitcointHandle.Rpc.Url));


            int blockNumner = await _bitcoinAggRepository.GetNextBlockId();
            int blockHeight = await _rpcClient.GetBlockCountAsync();
            while (blockNumner <= blockHeight - (_settings.NumberOfConfirm - 1))
            {
                List<WalletModel> wallets = new List<WalletModel>();
                var block = await _rpcClient.GetBlockAsync(blockNumner);
                var inTransactions = new List<String>();
                foreach (var transaction in block.Transactions)
                {
                    foreach (var txIn in transaction.Inputs)
                    {

                        var prevTx = txIn.PrevOut.Hash;
                        if (prevTx.ToString() == "0000000000000000000000000000000000000000000000000000000000000000")
                        {
                            continue;
                        }
                        uint prevN = txIn.PrevOut.N;
                        var pTx = (await _rpcClient.GetRawTransactionAsync(prevTx)).Outputs[prevN];
                        var address = pTx.ScriptPubKey.GetDestinationAddress(_rpcClient.Network)?.ToString();
                        if (string.IsNullOrEmpty(address))
                        {
                            continue;
                        }
                        inTransactions.Add(address);
                        
                        var oTx = (from t in transaction.Outputs
                            let otAddress = t.ScriptPubKey.GetDestinationAddress(_rpcClient.Network)?.ToString()
                            where otAddress != null && otAddress.Equals(address)
                            select t).FirstOrDefault();
                        if (oTx == null)
                        {
                            continue;
                        }

                        var delta = (double)(oTx.Value - pTx.Value).ToDecimal(MoneyUnit.BTC);
                        wallets.Add(new WalletModel{Address = address, AmountChange = delta, TransactionId = transaction.GetHash().ToString()});

                    }

                    foreach (var txOut in transaction.Outputs)
                    {
                        var outAddress = txOut.ScriptPubKey.GetDestinationAddress(_rpcClient.Network)?.ToString();
                        if (inTransactions.Any(itx => itx.Equals(outAddress)))
                        {
                            continue;
                        }

                       
                        var delta = (double)txOut.Value.ToDecimal(MoneyUnit.BTC);

                        wallets.Add(new WalletModel { Address = outAddress, AmountChange = delta, TransactionId = transaction.GetHash().ToString() });
                    }
                }

                await UpdateWallets(wallets, blockNumner);
                blockNumner++;
                await _bitcoinAggRepository.SetNextBlockId(blockNumner);
                blockHeight = await _rpcClient.GetBlockCountAsync();
                await UpdateCountDown(blockHeight - blockNumner);
            }
        }

        private async Task UpdateWallets(List<WalletModel> wallets, int blockNumner)
        {
            var ourResponse = await _payWalletService.GetLykkeWalletsWithHttpMessagesAsync(wallets.Select(w=>w.Address).ToList());
            var our = ourResponse?.Body as WalletResponseModel;
            if (our == null) return;

            var ourWallets = (from w in wallets
                join ow in our.Wallets on w.Address equals ow.WalletAddress
                select w).ToList();

            foreach (var ourWallet in ourWallets)
            {
                await _bitcoinAggRepository.SetTransactionAsync(new BitcoinAggEntity
                {
                    WalletAddress = ourWallet.Address,
                    TransactionId = ourWallet.TransactionId,
                    Amount = ourWallet.AmountChange,
                    BlockNumber = blockNumner
                });
            }

            if (_settings.NeedBroadcast && ourWallets.Count > 0)
            {
                await _bitcoinBroadcast.BroadcastMessage(new WalletMqModel{Wallets = ourWallets});
            }
        }

     
        private async Task UpdateCountDown(int i)
        {
            await _log.WriteInfoAsync("Lykke.Job.BitcoinTransactionAggregator", "Handle new block", "On block handled",
                $"Need to handle {i} block(s)");
        }


        
    }
}