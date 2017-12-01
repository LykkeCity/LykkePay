using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.Common.Entities.Pay;
using Lykke.Common.Entities.Security;
using Lykke.Core;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LykkePay.API.Controllers
{

    public class BaseController : Controller
    {
        protected readonly PayApiSettings PayApiSettings;
        protected readonly HttpClient HttpClient;


        public BaseController(PayApiSettings payApiSettings, HttpClient client)
        {
            PayApiSettings = payApiSettings;
            HttpClient = client;

        }

        protected string MerchantId => HttpContext.Request.Headers["Lykke-Merchant-Id"].ToString() ?? "";
        protected string MerchantSessionId => HttpContext.Request.Headers["Lykke-Merchant-Session-Id"].ToString() ?? "";
        protected IMerchantEntity Merchant => GetCurrentMerchant();

        private IMerchantEntity _merchant;
        private IMerchantEntity GetCurrentMerchant()
        {
            _merchant = _merchant ?? (_merchant = JsonConvert.DeserializeObject<MerchantEntity>(
                       HttpClient.GetAsync($"{PayApiSettings.Services.MerchantClientService}{MerchantId}").Result
                       .Content.ReadAsStringAsync().Result));

            return _merchant;
        }

        protected void StoreNewSessionId(string sessionId)
        {
            HttpContext.Response.Headers.Add("Lykke-Merchant-Session-Id", sessionId);
        }

        protected async Task<IActionResult> ValidateRequest()
        {
            string strToSign;
            Console.WriteLine($"Method {HttpContext.Request.Method}");
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
                strToSign = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path.ToString().TrimEnd('/')}{HttpContext.Request.QueryString}";
            }
            Console.WriteLine($"strToSign {strToSign}");
            var strToSend = JsonConvert.SerializeObject(new MerchantAuthRequest
            {
                MerchantId = MerchantId,
                StringToSign = strToSign,
                Sign = HttpContext.Request.Headers["Lykke-Merchant-Sign"].ToString() ?? ""
            });
            Console.WriteLine($"strToSend {strToSend}");
            var respone = await HttpClient.PostAsync(PayApiSettings.Services.MerchantAuthService, new StringContent(
                strToSend, Encoding.UTF8, "application/json"));
            var isValid = (SecurityErrorType)int.Parse(await respone.Content.ReadAsStringAsync());
            Console.WriteLine($"isValid {isValid}");
            if (isValid != SecurityErrorType.Ok)
            {
                switch (isValid)
                {
                    case SecurityErrorType.AssertEmpty:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    case SecurityErrorType.MerchantUnknown:
                    case SecurityErrorType.SignEmpty:
                        return BadRequest();
                    case SecurityErrorType.SignIncorrect:
                        return Forbid();
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            return Ok();
        }



        protected async Task<object> GetRate(string assertId)
        {
            List<AssertPairRate> rates;
            var newSessionId = string.Empty;
            try
            {
                var rateServiceUrl = $"{PayApiSettings.Services.PayServiceService}?sessionId={MerchantSessionId}&cacheTimeout={Merchant?.TimeCacheRates}";

                var response = JsonConvert.DeserializeObject<AssertListWithSession>(
                    await(await HttpClient.GetAsync(rateServiceUrl)).Content
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
                    return NotFound();
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var rate = rates.First(r => r.AssetPair.Equals(assertId, StringComparison.CurrentCultureIgnoreCase));
            return new AssertPairRateWithSession(rate, newSessionId);
        }

        protected double CalculateValue(double value, int accuracy, AprRequest request, bool isPluse)
        {
            var origValue = value;
            var spread = value * (Merchant.DeltaSpread/100);
            value = isPluse ? (value + spread) : (value - spread);
            double lpFee = value * (Merchant.LpMarkupPercent < 0 ? PayApiSettings.LpMarkup.Percent/100 : Merchant.LpMarkupPercent / 100f);
            double lpPips = Math.Pow(10, -1 * accuracy) * Merchant.LpMarkupPips < 0 ? PayApiSettings.LpMarkup.Pips : Merchant.LpMarkupPips;

            var delta = spread + lpFee + lpPips;

            if (request != null)
            {
                var fee = value * (request.Percent / 100f);
                var pips =  Math.Pow(10, -1 * accuracy) * request.Pips;

                delta += fee + pips;
            }

            var result = origValue + (isPluse ? delta : -delta);

            var powRound = Math.Pow(10, -1 * accuracy) * 0.5;

            result += !isPluse ? powRound : -powRound;
            var res =  Math.Round(result, accuracy);
            int mult = (int)Math.Pow(10, accuracy);


            res = Math.Ceiling(res * mult) / mult;

            if (res < 0)
            {
                res = 0;
            }

            return res;

        }
    }
}