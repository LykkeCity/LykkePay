using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Pay.Service.StoreRequest.Client;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Mvc;


namespace LykkePay.API.Controllers
{
    [Route("api/Transfer")]
    public class TransferController : BaseController
    {
        private readonly ILykkePayServiceStoreRequestMicroService _storeRequestClient;
        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TransferRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            var store = request.GetRequest();
            store.MerchantId = MerchantId;

            await _storeRequestClient.ApiStorePostWithHttpMessagesAsync(store);

            return Content(store.RequestId);
        }

        public TransferController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient) : base(payApiSettings, client)
        {
            _storeRequestClient = storeRequestClient;
        }
    }


       
}
