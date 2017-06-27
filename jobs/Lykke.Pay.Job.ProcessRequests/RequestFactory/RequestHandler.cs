using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Core;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    public abstract class RequestHandler : IRequestHandler
    {
        protected IMerchantPayRequest MerchantPayRequest { get; set; }

        protected RequestHandler(IMerchantPayRequest payRequest)
        {
            MerchantPayRequest = payRequest;
        }

        public static IRequestHandler Create(IMerchantPayRequest payRequest)
        {
            switch (payRequest.MerchantPayRequestType)
            {
                case MerchantPayRequestType.ExchangeTransfer:
                    return new ExchangeTransferRequestHandler(payRequest);
                case MerchantPayRequestType.Purchase:
                    return new PurchaseRequestHandler(payRequest);
                case MerchantPayRequestType.Transfer:
                    return new TransferRequestHandler(payRequest);
            }

            return null;
        }

        public abstract Task Handle();

    }
}
