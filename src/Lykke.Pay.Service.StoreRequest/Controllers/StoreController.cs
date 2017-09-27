using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.Core;
using Lykke.Pay.Service.StoreRequest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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
        public async Task<IActionResult> Post([FromBody]PayRequest request)
        {
            await _merchantPayRequestRepository.SaveRequestAsync(request);
            return Content(request.RequestId);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(await _merchantPayRequestRepository.GetAllAsync());
        }

        [HttpGet("{merchantId}")]
        public async Task<IActionResult> GetByMerchantId(string merchantId)
        {
            return Json(await _merchantPayRequestRepository.GetAllByMerchantIdAsync(merchantId));
        }

    }
}
