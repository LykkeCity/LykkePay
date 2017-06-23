using System;
using System.Threading.Tasks;
using Lykke.Core;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    class PurchaseRequestHandler : RequestHandler
    {
        public override async Task Handle()
        {
            throw new NotImplementedException();
        }

        public PurchaseRequestHandler(IMerchantPayRequest payRequest) : base(payRequest)
        {
        }
    }
}