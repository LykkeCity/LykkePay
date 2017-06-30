using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lykke.Common.Entities.Security;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.GenerateAddress.Client.Models;
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
        private readonly ILykkePayServiceGenerateAddressMicroService _gaService;
        public GenerateAddressController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceGenerateAddressMicroService gaService) : base(payApiSettings, client)
        {
            _gaService = gaService;
        }

        [HttpGet("{assertId}")]
        public async Task<IActionResult> Get(string assertId)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            var response = await _gaService.ApiGeneratePostWithHttpMessagesAsync(new GenerateAddressRequest
            {
                MerchantId = MerchantId,
                AssertId = assertId
            });
            

            var publicKey = response.Body.Address;

            var result = new
            {
                Currency = assertId,
                Address = publicKey
            };
            return Json(result);
        }
        
    }
}
