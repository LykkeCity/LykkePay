using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Core;
using Lykke.Pay.Service.StoreRequest.Client;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    public abstract class RequestHandler : IRequestHandler
    {
        protected IMerchantPayRequest MerchantPayRequest { get; set; }
        protected LykkePayJobProcessRequestsSettings Settings { get; set; }


        protected RequestHandler(IMerchantPayRequest payRequest, LykkePayJobProcessRequestsSettings settings)
        {
            MerchantPayRequest = payRequest;
            Settings = settings;
        }

        public static IRequestHandler Create(IMerchantPayRequest payRequest, LykkePayJobProcessRequestsSettings settings)
        {
            switch (payRequest.MerchantPayRequestType)
            {
                case MerchantPayRequestType.ExchangeTransfer:
                    return new ExchangeTransferRequestHandler(payRequest, settings);
                case MerchantPayRequestType.Purchase:
                    return new PurchaseRequestHandler(payRequest, settings);
                case MerchantPayRequestType.Transfer:
                    return new TransferRequestHandler(payRequest, settings);
            }

            return null;
        }

        public abstract Task Handle();

    }
}
