using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Common.Entities.Pay;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{
    //[Produces("application/json")]
    [Route("api/Purchase")]
    public class PurchaseController : BaseController
    {
        public PurchaseController(PayApiSettings payApiSettings, HttpClient client) : base(payApiSettings, client)
        {
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }



            throw new NotImplementedException();
        }
    }
}