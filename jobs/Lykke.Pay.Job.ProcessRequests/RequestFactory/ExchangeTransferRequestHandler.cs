using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Core;
using Lykke.Pay.Service.StoreRequest.Client;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    class ExchangeTransferRequestHandler : RequestHandler
    {
        public override async Task Handle()
        {
            await Task.Delay(10);
        }

        public ExchangeTransferRequestHandler(IMerchantPayRequest payRequest, LykkePayJobProcessRequestsSettings settings) : base(payRequest, settings)
        {
        }
    }
}
