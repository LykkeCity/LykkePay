using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Lykke.Common.Entities.Pay;
using Lykke.Core;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using Lykke.Service.ExchangeOperations.Client;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GenerateAddressRequest = Lykke.Pay.Service.GenerateAddress.Client.Models.GenerateAddressRequest;

namespace LykkePay.API.Controllers
{

    [Route("api/Order")]
    public class OrderController : BaseTransactionController
    {
        private readonly ILykkePayServiceGenerateAddressMicroService _gaService;
        private readonly ILykkePayServiceStoreRequestMicroService _storeRequestClient;

        public OrderController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient, IBitcoinApi bitcointApiClient,
            ILykkePayServiceGenerateAddressMicroService generateAddressClient, IExchangeOperationsServiceClient exchangeOperationClient, IBitcoinAggRepository bitcoinAddRepository)
            : base(payApiSettings, client, generateAddressClient, storeRequestClient, bitcointApiClient, bitcoinAddRepository)
        {

            _gaService = generateAddressClient;
            _storeRequestClient = storeRequestClient;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }
            var store = request?.GetRequest(MerchantId);
            if (store == null)
            {
                return BadRequest();
            }

            var resp = await _gaService.ApiGeneratePostWithHttpMessagesAsync(new GenerateAddressRequest
            {
                MerchantId = MerchantId,
                AssertId = store.AssetId
            });


            store.SourceAddress = resp.Body.Address;

            var result = await GetRate(store.AssetPair);

            var post = result as StatusCodeResult;
            if (post != null)
            {
                return post;
            }

            var rate = (AssertPairRateWithSession)result;

            var arpRequest = new AprRequest
            {
                Markup = new AssertPairRateRequest
                {
                    Percent = (float) (store.Markup.Percent ?? 0),
                    Pips = store.Markup.Pips ?? 0
                }
                
            };

            //rate.Bid = (float)CalculateValue(rate.Bid, rate.Accuracy, arpRequest, false);
            rate.Ask = (float)CalculateValue(rate.Ask, rate.Accuracy, arpRequest, true);

            await _storeRequestClient.ApiStoreOrderPostWithHttpMessagesAsync(store);

            

            return Json(new OrderRequestResponse(store, rate.Ask));
         


        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetStatus(string id)
        {
            return await GetTransactionStatus(id);
        }

        [HttpPost("{id}/successUrl")]
        public async Task<IActionResult> UpdateSucecessUrl(string id, [FromBody] UrlRequest url)
        {
            var result = await UpdateUrl(id, url.Url, UrlType.Success);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500);
        }

        [HttpPost("{id}/progressUrl")]
        public async Task<IActionResult> UpdateProgressUrl(string id, [FromBody] UrlRequest url)
        {
            var result = await UpdateUrl(id, url.Url, UrlType.InProgress);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500);
        }

        [HttpPost("{id}/errorUrl")]
        public async Task<IActionResult> UpdateErrorUrl(string id, [FromBody] UrlRequest url)
        {
            var result = await UpdateUrl(id, url.Url, UrlType.Error);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500);
        }
    }
}