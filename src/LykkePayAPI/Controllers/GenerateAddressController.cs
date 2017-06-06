using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            byte[] publicKey = new byte[1024 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(publicKey);
            }
            var result = new
            {
                Currency = assertId,
                Address = Convert.ToBase64String(publicKey)
            };
            return Json(result);
        }
        
    }
}
