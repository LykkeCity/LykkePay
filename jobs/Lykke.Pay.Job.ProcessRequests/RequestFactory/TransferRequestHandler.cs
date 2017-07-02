using System;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.AzureRepositories.Azure.Tables;
using Lykke.Core;
using Lykke.Pay.Service.StoreRequest.Client;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    class TransferRequestHandler : RequestHandler
    {
        private readonly IBitcoinAggRepository _bitcoinRepo;
        private readonly IMerchantPayRequestRepository _merchantPayRequestRepository;
        public override async Task Handle()
        {
            
            if (MerchantPayRequest.MerchantPayRequestStatus != MerchantPayRequestStatus.InProgress)
            {
                return;
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
        }

       
    }
}