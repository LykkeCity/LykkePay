using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lykke.Common.Entities.Pay;
using Lykke.Common.Entities.Security;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LykkePay.API.Controllers
{
    [Produces("application/json")]
    [Route("api/assetPairRates")]
    public class AssetPairRatesController : Controller
    {
        private readonly PayApiSettings _payApiSettings;
        private readonly HttpClient _client;

        public AssetPairRatesController(PayApiSettings payApiSettings, HttpClient client)
        {
            _payApiSettings = payApiSettings;
            _client = client;
        }

        private async Task<SecurityErrorType> ValidateRequest()
        {
            string strToSign;

            if (HttpContext.Request.Method.Equals("POST"))
            {
                HttpContext.Request.EnableRewind();
                HttpContext.Request.Body.Position = 0;
                using (StreamReader reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    strToSign = reader.ReadToEnd();
                }
                HttpContext.Request.Body.Position = 0;
            }
            else
            {
                strToSign = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}";
            }

            var respone = await _client.PostAsync(_payApiSettings.Services.MerchantAuthService, new StringContent(
                JsonConvert.SerializeObject(new MerchantAuthRequest
                {
                    MerchantId = HttpContext.Request.Headers["Lykke-Merchant-Id"].ToString() ?? "",
                    StringToSign = strToSign,
                    Sign = HttpContext.Request.Headers["Lykke-Merchant-Sign"].ToString() ?? ""
                }), Encoding.UTF8, "application/json"));
            return (SecurityErrorType)int.Parse(await respone.Content.ReadAsStringAsync());
        }


        [HttpGet("{assertId}")]
        public async Task<IActionResult> Get(string assertId)
        {

            List<AssertPairRate> rates;

            try
            {
                rates = JsonConvert.DeserializeObject<List<AssertPairRate>>(
                    await (await _client.GetAsync(_payApiSettings.Services.PayServiceService)).Content
                        .ReadAsStringAsync());

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
            var isValude = await ValidateRequest();
            if (isValude != SecurityErrorType.Ok)
            {
                switch (isValude)
                {
                    case SecurityErrorType.AssertEmpty:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    case SecurityErrorType.MerchantUnknown:
                    case SecurityErrorType.SignEmpty:
                        return BadRequest();
                    case SecurityErrorType.SignIncorrect:
                        return Forbid();
                }
            }

            List<AssertPairRate> rates;

            try
            {
                rates = JsonConvert.DeserializeObject<List<AssertPairRate>>(
                    await (await _client.GetAsync(_payApiSettings.Services.PayServiceService)).Content
                        .ReadAsStringAsync());

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

            return new JsonResult(rate);
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