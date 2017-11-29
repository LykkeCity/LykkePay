using System.Threading.Tasks;
using Lykke.Core;
using Lykke.Pay.Service.StoreRequest.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Pay.Service.StoreRequest.Controllers
{
    [Route("api/StoreOrder")]
    public class StoreOrderController : Controller
    {

        private readonly IMerchantOrderRequestRepository _merchantOrderRequestRepository;
        public StoreOrderController(IMerchantOrderRequestRepository merchantOrderRequestRepository)
        {
            _merchantOrderRequestRepository = merchantOrderRequestRepository;
        }

        // POST api/StoreOrder
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]OrderRequest request)
        {
            await _merchantOrderRequestRepository.SaveRequestAsync(request);
            return Content(request.RequestId);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(await _merchantOrderRequestRepository.GetAllAsync());
        }

        [HttpGet("{merchantId}")]
        public async Task<IActionResult> GetByMerchantId(string merchantId)
        {
            return Json(await _merchantOrderRequestRepository.GetAllByMerchantIdAsync(merchantId));
        }

    }
}