using System;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Pay.Service.StoreRequest.Client;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LykkePay.API.Controllers
{
    [Route("api/ExchangeTransfer")]
    public class ExchangeTransferController : BaseController
    {
        private readonly ILykkePayServiceStoreRequestMicroService _storeRequestClient;
        public ExchangeTransferController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient) : base(payApiSettings, client)
        {
            _storeRequestClient = storeRequestClient;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ExchangeTransferRequest request)
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
    }
}