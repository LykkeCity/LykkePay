using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
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

            var strToSend = JsonConvert.SerializeObject(new MerchantAuthRequest
            {
                MerchantId = MerchantId,
                StringToSign = strToSign,
                Sign = HttpContext.Request.Headers["Lykke-Merchant-Sign"].ToString() ?? ""
            });
            var respone = await HttpClient.PostAsync(PayApiSettings.Services.MerchantAuthService, new StringContent(
                strToSend, Encoding.UTF8, "application/json"));
            var isValid = (SecurityErrorType)int.Parse(await respone.Content.ReadAsStringAsync());
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

        protected float CalculateValue(float value, int accuracy, AprRequest request, bool isPluse)
        {
            var spread = value * (float)Merchant.DeltaSpread;
            float lpFee = value * (PayApiSettings.LpMarkup.Percent / 100f);
            float lpPips = (float)Math.Pow(10, -1 * accuracy) * PayApiSettings.LpMarkup.Pips;

            var delta = spread + lpFee + lpPips;

            if (request != null)
            {
                var fee = value * (request.Percent / 100f);
                var pips = (float) Math.Pow(10, -1 * accuracy) * request.Pips;

                delta += fee + pips;
            }

            var result = value + (isPluse ? delta : -delta);

            var powRound = Math.Pow(10, -1 * accuracy) * 0.5;

            return (float)Math.Round(result + (isPluse ? powRound : -powRound), accuracy);

        }
    }
}