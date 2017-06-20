using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.Core;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Pay.Service.StoreRequest.Controllers
{
    [Route("api/Store")]
    public class StoreController : Controller
    {
       
        private readonly IMerchantPayRequestRepository _merchantPayRequestRepository;
        public StoreController(IMerchantPayRequestRepository merchantPayRequestRepository)
        {
            _merchantPayRequestRepository = merchantPayRequestRepository;
        }

        // POST api/Store
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]MerchantPayRequest request)
        {
            await _merchantPayRequestRepository.SaveRequestAsync(request);
            return Content(request.RequestId);
        }

       
    }
}
