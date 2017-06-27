using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Common.Entities.Pay;
using Lykke.Pay.Service.StoreRequest.Client;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{
    //[Produces("application/json")]
    [Route("api/Purchase")]
    public class PurchaseController : BaseController
    {
        private readonly ILykkePayServiceStoreRequestMicroService _storeRequestClient;
        public PurchaseController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient) : base(payApiSettings, client)
        {
            _storeRequestClient = storeRequestClient;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            var store = request.GetRequest();
            store.MerchantId = MerchantId;

            await _storeRequestClient.ApiStorePostAsync(store);

            return Content(store.RequestId);

            
        }
    }
}