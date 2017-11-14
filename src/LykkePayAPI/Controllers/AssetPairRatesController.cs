using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Common.Entities.Pay;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LykkePay.API.Controllers
{
    [Produces("application/json")]
    [Route("api/assetPairRates")]
    public class AssetPairRatesController : BaseController
    {
       

        public AssetPairRatesController(PayApiSettings payApiSettings, HttpClient client) 
            : base(payApiSettings, client)
        {
           
        }

       


        [HttpGet("{assertId}")]
        public async Task<IActionResult> Get(string assertId)
        {

            List<AssertPairRate> rates;

            try
            {
                var rateServiceUrl = $"{PayApiSettings.Services.PayServiceService}?sessionId={MerchantSessionId}&cacheTimeout={Merchant?.TimeCacheRates}";

                var response = JsonConvert.DeserializeObject<AssertListWithSession>(
                    await (await HttpClient.GetAsync(rateServiceUrl)).Content
                        .ReadAsStringAsync());

               // var newSessionId = response.SessionId;
                rates = response.Asserts;

             
                if (!rates.Any(r => r.AssetPair.Equals(assertId, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var rate = rates.First(r => r.AssetPair.Equals(assertId, StringComparison.CurrentCultureIgnoreCase));

            return Json(rate);
        }

        // POST api/assetPairRates/assertId
        [HttpPost("{assertId}")]
        public async Task<IActionResult> Post([FromBody]AssertPairRateRequest request, string assertId)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            List<AssertPairRate> rates;
            string newSessionId;
            try
            {
                var rateServiceUrl = $"{PayApiSettings.Services.PayServiceService}?sessionId={MerchantSessionId}&cacheTimeout={Merchant?.TimeCacheRates}";
                
                var response = JsonConvert.DeserializeObject<AssertListWithSession>(
                    await (await HttpClient.GetAsync(rateServiceUrl)).Content
                        .ReadAsStringAsync());

                newSessionId = response.SessionId;
                rates = response.Asserts;

                if (!string.IsNullOrEmpty(MerchantSessionId) && !MerchantSessionId.Equals(newSessionId))
                {
                    throw new InvalidDataException("Session expired");
                }

                StoreNewSessionId(newSessionId);

                if (!rates.Any(r => r.AssetPair.Equals(assertId, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var rate = rates.First(r => r.AssetPair.Equals(assertId, StringComparison.CurrentCultureIgnoreCase));
            rate.Bid = CalculateValue(rate.Bid, rate.Accuracy, request, true);
            rate.Ask = CalculateValue(rate.Ask, rate.Accuracy, request, false);

            
            return new JsonResult(new AssertPairRateWithSession(rate, newSessionId));
        }

        private float CalculateValue(float value, int accuracy, AssertPairRateRequest request, bool isPluse)
        {
            float fee = value * (request.Percent / 100f);
            fee += (float)Math.Pow(10, -1 * accuracy) * request.Pips;
            if (isPluse)
            {
                return value + fee;
            }
            return value - fee;
        }



    }
}