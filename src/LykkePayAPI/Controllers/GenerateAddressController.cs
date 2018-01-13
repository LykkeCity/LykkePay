using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
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
    [Route("api/v1/generateAddress")]
    public class GenerateAddressController : BaseController
    {
        private readonly ILykkePayServiceGenerateAddressMicroService _gaService;

        public GenerateAddressController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceGenerateAddressMicroService gaService,
            ILog log) : base(payApiSettings, client, log)
        {
            _gaService = gaService;
        }

        //todo: надо заменить на пост, нельзя по гету выполнять действия или менять состояние!!!
        [HttpGet("{assertId}")]
        public async Task<IActionResult> GenerateAddress(string assertId)
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

            var context = new {MerchantId, AssertId = assertId, Address = publicKey };
            await Log.WriteInfoAsync(nameof(GenerateAddressController), nameof(GenerateAddress), context.ToJson(), $"Generate address for mercahnt" );
            
            return Json(result);
        }
        
    }
}
