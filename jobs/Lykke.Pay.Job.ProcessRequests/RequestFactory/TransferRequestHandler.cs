using System;
using System.Net;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Bitcoint.Api.Client.Models;
using Common.Log;
using Lykke.AzureRepositories;
using Lykke.AzureRepositories.Azure.Tables;
using Lykke.AzureRepositories.Log;
using Lykke.Core;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    class TransferRequestHandler : RequestHandler
    {
        private readonly IBitcoinAggRepository _bitcoinRepo;
        private readonly IMerchantPayRequestRepository _merchantPayRequestRepository;
        private readonly IBitcoinApi _bitcoinApi;
        public override async Task Handle()
        {
            
            if (MerchantPayRequest.MerchantPayRequestStatus != MerchantPayRequestStatus.InProgress)
            {
                return;
            }

            Guid gTransactionId;
            if (Guid.TryParse(MerchantPayRequest.TransactionId, out gTransactionId))
            {
                var resp = await _bitcoinApi.ApiTransactionByTransactionIdGetWithHttpMessagesAsync(gTransactionId);
                var trnResponce = resp.Body as TransactionHashResponse;
                if (resp.Response.StatusCode != HttpStatusCode.OK || trnResponce == null)
                {
                    return;
                }

                MerchantPayRequest.TransactionId = trnResponce.TransactionHash;
                await _merchantPayRequestRepository.SaveRequestAsync(MerchantPayRequest);
            }

            var transaction = await _bitcoinRepo.GetWalletTransactionAsync(MerchantPayRequest.DestinationAddress, MerchantPayRequest.TransactionId);

            if (transaction == null)
            {
                return;
            }

            MerchantPayRequest.MerchantPayRequestNotification |= MerchantPayRequestNotification.Success;
            MerchantPayRequest.MerchantPayRequestStatus = MerchantPayRequestStatus.Completed;
            await _merchantPayRequestRepository.SaveRequestAsync(MerchantPayRequest);
        }

        public TransferRequestHandler(IMerchantPayRequest payRequest, LykkePayJobProcessRequestsSettings settings) : base(payRequest, settings)
        {
            _bitcoinRepo =
                new BitcoinAggRepository(
                    new AzureTableStorage<BitcoinAggEntity>(
                        settings.Db.MerchantWalletConnectionString, "BitcoinAgg",
                        null),
                    new AzureTableStorage<BitcoinHeightEntity>(
                        settings.Db.MerchantWalletConnectionString, "BitcoinHeight",
                        null));
            _merchantPayRequestRepository =
                new MerchantPayRequestRepository(
                    new AzureTableStorage<MerchantPayRequest>(settings.Db.MerchantWalletConnectionString, "MerchantPayRequest", null));

            //"http://52.164.252.39/"
            _bitcoinApi = new BitcoinApi(new Uri(settings.Services.BitcoinApiService));
        }

       
    }
}