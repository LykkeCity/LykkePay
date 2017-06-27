using System;
using System.Threading.Tasks;
using Lykke.Core;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    class TransferRequestHandler : RequestHandler
    {
        public override async Task Handle()
        {
            throw new NotImplementedException();
        }

        public TransferRequestHandler(IMerchantPayRequest payRequest) : base(payRequest)
        {
        }
    }
}