using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lykke.Common.Entities.Pay;
using Lykke.Common.Entities.Security;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LykkePay.API.Controllers
{
    [Produces("application/json")]
    [Route("api/generateAddress")]
    public class GenerateAddressController : BaseController
    {
        public GenerateAddressController(PayApiSettings payApiSettings, HttpClient client) : base(payApiSettings, client)
        {

        }

        [HttpGet("{assertId}")]
        public async Task<IActionResult> Get(string assertId)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            var respone = await HttpClient.PostAsync(PayApiSettings.Services.GenerateAddressService, new StringContent(
                JsonConvert.SerializeObject(new GenerateAddressRequest
                {
                    MerchantId = HttpContext.Request.Headers["Lykke-Merchant-Id"].ToString() ?? "",
                    AssertId = assertId
                }), Encoding.UTF8, "application/json"));

            if (respone.StatusCode != HttpStatusCode.OK)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var publicKey = await respone.Content.ReadAsStringAsync();

            var result = new
            {
                Currency = assertId,
                Address = publicKey
            };
            return Json(result);
        }
        
    }
}
