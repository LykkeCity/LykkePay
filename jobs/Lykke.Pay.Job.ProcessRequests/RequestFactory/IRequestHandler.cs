using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Pay.Job.ProcessRequests.RequestFactory
{
    public interface IRequestHandler
    {
        Task Handle();
    }
}
