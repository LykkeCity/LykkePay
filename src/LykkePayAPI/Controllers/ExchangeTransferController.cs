using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Lykke.Common.Entities.Pay;
using Lykke.Core;
using Lykke.Pay.Common;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LykkePay.API.Controllers
{
    [Route("api/v1/convert/transfer")]
    public class ExchangeTransferController : BaseTransactionController
    {

        public ExchangeTransferController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient,
            IBitcoinApi bitcointApiClient, ILykkePayServiceGenerateAddressMicroService generateAddressClient, IBitcoinAggRepository bitcoinAddRepository) 
            : base(payApiSettings, client, generateAddressClient, storeRequestClient, bitcointApiClient, bitcoinAddRepository)
        {
            
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ExchangeTransferRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            if (request.PaidAmount <= 0)
            {
                return Json(
                    new TransferErrorReturn
                    {
                        TransferResponse = new TransferErrorResponse
                        {
                            TransferError = TransferError.INVALID_AMOUNT,
                            TimeStamp = DateTime.UtcNow.Ticks
                        }
                    });
            }

            var rateServiceUrl = $"{PayApiSettings.Services.PayServiceService}?sessionId={MerchantSessionId}&cacheTimeout={Merchant?.TimeCacheRates}";

            var response = JsonConvert.DeserializeObject<AssertListWithSession>(
                await (await HttpClient.GetAsync(rateServiceUrl)).Content
                    .ReadAsStringAsync());

            var newSessionId = response.SessionId;
            var rates = response.Asserts;

            if (!string.IsNullOrEmpty(MerchantSessionId) && !MerchantSessionId.Equals(newSessionId))
            {
                throw new InvalidDataException("Session expired");
            }

            StoreNewSessionId(newSessionId);

           
            var store = request.GetRequest();
            var result = await GetRate(store.AssetPair);

            var post = result as StatusCodeResult;
            if (post != null)
            {
                return null;
            }

            var rate = (AssertPairRateWithSession)result;

            var arpRequest = new AprRequest
            {
                Markup = new Lykke.Contracts.Pay.AssertPairRateRequest
                {
                    Percent = store.Markup.Percent ?? 0,
                    Pips = store.Markup.Pips ?? 0
                }

            };

            //rate.Bid = (float)CalculateValue(rate.Bid, rate.Accuracy, arpRequest, false);
            rate.Ask = (float)CalculateValue(rate.Ask, rate.Accuracy, arpRequest, true);
            store.Amount = store.Amount / rate.Ask;

            return await PostTransfer(request.AssetPair.Replace(request.BaseAsset, string.Empty), store);
           
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