using System;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Core;
using Lykke.Pay.Service.StoreRequest.Client;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    class PurchaseRequestHandler : RequestHandler
    {
        public override async Task Handle()
        {
            await Task.Delay(10);
        }

        public PurchaseRequestHandler(IMerchantPayRequest payRequest, LykkePayJobProcessRequestsSettings settings) : base(payRequest, settings)
        {
        }
    }
}