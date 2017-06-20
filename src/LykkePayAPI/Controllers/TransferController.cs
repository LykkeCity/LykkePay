using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Mvc;


namespace LykkePay.API.Controllers
{
    [Route("api/Transfer")]
    public class TransferController : BaseController
    {
       
        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TransferRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }



            throw new NotImplementedException();
        }

        public TransferController(PayApiSettings payApiSettings, HttpClient client) : base(payApiSettings, client)
        {
        }
    }


       
}
