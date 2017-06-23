using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Core;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    class ExchangeTransferRequestHandler : RequestHandler
    {
        public override async Task Handle()
        {
            throw new NotImplementedException();
        }

        public ExchangeTransferRequestHandler(IMerchantPayRequest payRequest) : base(payRequest)
        {
        }
    }
}
