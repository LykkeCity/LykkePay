using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.AzureRepositories.Azure.Tables;
using Lykke.Common.Entities.Pay;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using NBitcoin.RPC;
using NBitcoin.SPV;
using Lykke.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.Pay.Job.BitcointBlocksHandle
{
    class Program
    {


        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                await MainAsync(args);
            }).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");


            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();
            var connectionString = configuration.GetValue<string>("ConnectionString");
            var generalSettings = SettingsReader.SettingsReader.ReadGeneralSettings<Settings>(connectionString);

            IBitcoinAggRepository bitcoinRepo =
                new BitcoinAggRepository(
                    new AzureTableStorage<BitcoinAggEntity>(
                        generalSettings.LykkePayJobBitcointHandle.Db.MerchantWalletConnectionString, "BitcoinAgg",
                        null),
                    new AzureTableStorage<BitcoinHeightEntity>(
                        generalSettings.LykkePayJobBitcointHandle.Db.MerchantWalletConnectionString, "BitcoinHeight",
                        null));
            IMerchantWalletRepository merchantWalletRepository =
                new MerchantWalletRepository(new AzureTableStorage<MerchantWalletEntity>(
                    generalSettings.LykkePayJobBitcointHandle.Db.MerchantWalletConnectionString, "MerchantWallets",
                    null));
            IMerchantWalletHistoryRepository merchantWalletHistoryRepository =
                new MerchantWalletHistoryRepository(new AzureTableStorage<MerchantWalletHistoryEntity>(
                    generalSettings.LykkePayJobBitcointHandle.Db.MerchantWalletConnectionString,
                    "MerchantWalletsHistory", null));

            var client =
                new RPCClient(
                    new NetworkCredential(generalSettings.LykkePayJobBitcointHandle.Rpc.UserName,
                        generalSettings.LykkePayJobBitcointHandle.Rpc.Password),
                    new Uri(generalSettings.LykkePayJobBitcointHandle.Rpc.Url));

           
            int blockNumner = await bitcoinRepo.GetNextBlockId();
            int blockHeight = await client.GetBlockCountAsync();
            blockNumner = 1152735;
            while (blockNumner <= blockHeight - (generalSettings.LykkePayJobBitcointHandle.NumberOfConfirm - 1))
            {
                var wallets = await GetWallets(merchantWalletRepository,
                    generalSettings.LykkePayJobBitcointHandle.DataEncriptionPassword);
                var block = await client.GetBlockAsync(blockNumner);
                var inTransactions = new List<String>();
                foreach (var transaction in block.Transactions)
                {
                    var txId = transaction.GetHash().ToString();

                    foreach (var txIn in transaction.Inputs)
                    {

                        var prevTx = txIn.PrevOut.Hash;
                        if (prevTx.ToString() == "0000000000000000000000000000000000000000000000000000000000000000")
                        {
                            continue;
                        }
                        uint prevN = txIn.PrevOut.N;
                        var pTx = (await client.GetRawTransactionAsync(prevTx)).Outputs[prevN];
                        var address = pTx.ScriptPubKey.GetDestinationAddress(client.Network)?.ToString();
                        if (string.IsNullOrEmpty(address))
                        {
                            continue;
                        }
                        inTransactions.Add(address);
                        var wAddress = wallets.FirstOrDefault(w => address.Equals(w.Item2.Address));
                        if (wAddress == null)
                        {
                            continue;
                        }

                        var oTx = (from t in transaction.Outputs
                            let otAddress = t.ScriptPubKey.GetDestinationAddress(client.Network).ToString()
                            where otAddress.Equals(wAddress.Item2.Address)
                            select t).FirstOrDefault();
                        if (oTx == null)
                        {
                            continue;
                        }

                        var delta = (double) (oTx.Value - pTx.Value).ToDecimal(MoneyUnit.BTC);

                        await ChangeWalletAmount(bitcoinRepo, merchantWalletRepository, merchantWalletHistoryRepository,
                            generalSettings.LykkePayJobBitcointHandle.DataEncriptionPassword,
                            wAddress, transaction.GetHash().ToString(), delta, blockNumner);

                    }

                    foreach (var txOut in transaction.Outputs)
                    {
                        var outAddress = txOut.ScriptPubKey.GetDestinationAddress(client.Network)?.ToString();
                        if (inTransactions.Any(itx => itx.Equals(outAddress)))
                        {
                            continue;
                        }

                        var wAddress = wallets.FirstOrDefault(w => w.Item2.Address.Equals(outAddress));
                        if (wAddress == null)
                        {
                            continue;
                        }

                        var delta = (double)txOut.Value.ToDecimal(MoneyUnit.BTC);

                        await ChangeWalletAmount(bitcoinRepo, merchantWalletRepository, merchantWalletHistoryRepository,
                            generalSettings.LykkePayJobBitcointHandle.DataEncriptionPassword,
                            wAddress, transaction.GetHash().ToString(), delta, blockNumner);
                    }
                }

                blockNumner++;
                await bitcoinRepo.SetNextBlockId(blockNumner);
                blockHeight = await client.GetBlockCountAsync();
                UpdateCountDown(blockHeight - blockNumner);
            }
        }

        private static int prevValue = 0;
        private static void UpdateCountDown(int i)
        {
            var spaces = prevValue.ToString().Length - i.ToString().Length;
            if (spaces < 0)
            {
                spaces = 0;
            }
            string str = spaces > 0 ? new String(' ', spaces) : string.Empty;
            Console.Write($"\r{i}{str}");
            prevValue = i;
        }


        private static async Task ChangeWalletAmount(IBitcoinAggRepository bitcoinRepo, IMerchantWalletRepository merchantWalletRepository,
            IMerchantWalletHistoryRepository merchantWalletHistoryRepository, string password, Tuple<string, AssertPrivKeyPair> wAddress, string transactionId, double delta, int blockNumner)
        {
            await bitcoinRepo.SetTransactionAsync(new BitcoinAggEntity
            {
                WalletAddress = wAddress.Item2.Address,
                TransactionId = transactionId,
                Amount = delta,
                BlockNumber = blockNumner
            });

            var wallInt = wAddress.Item2;
            wallInt.Amount += delta;

            var encriptedData = EncryptData(JsonConvert.SerializeObject(wallInt), password);
            await merchantWalletRepository.SaveNewAddressAsync(new MerchantWalletEntity
            {
                MerchantId = wAddress.Item1,
                WalletAddress = wallInt.Address,
                Data = encriptedData
            });

            await merchantWalletHistoryRepository.SaveNewChangeRequestAsync(wallInt.Address, delta, "NA", "Aggregator");
        }



        private static async Task<List<Tuple<string, AssertPrivKeyPair>>> GetWallets(IMerchantWalletRepository merchantWalletRepository, string password)
        {
            var wallets = await merchantWalletRepository.GetAllAddressAsync();
            return (from w in wallets
                    select new Tuple<string, AssertPrivKeyPair>(w.MerchantId, JsonConvert.DeserializeObject<AssertPrivKeyPair>(DecryptData(w.Data, password)))).ToList();
        }

        private static string DecryptData(string data, string password)
        {

            byte[] result;
            using (var aes = Aes.Create())
            using (var md5 = MD5.Create())
            using (var sha256 = SHA256.Create())
            {
                aes.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                aes.IV = md5.ComputeHash(Encoding.UTF8.GetBytes(password));

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(Convert.FromBase64String(data)))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    result = resultStream.ToArray();
                }
            }

            return Encoding.UTF8.GetString(result);
        }

        private static string EncryptData(string data, string password)
        {

            byte[] result;
            using (var aes = Aes.Create())
            using (var md5 = MD5.Create())
            using (var sha256 = SHA256.Create())
            {
                aes.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                aes.IV = md5.ComputeHash(Encoding.UTF8.GetBytes(password));

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    result = resultStream.ToArray();
                }
            }

            return Convert.ToBase64String(result);
        }
    }


}