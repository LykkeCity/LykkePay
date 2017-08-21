using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{
    [Produces("application/json")]
    [Route("api/getBalance")]
    public class BalanceController : BaseTransactionController
    {

        public BalanceController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceGenerateAddressMicroService gnerateAddressClient, ILykkePayServiceStoreRequestMicroService storeRequestClient, IBitcoinApi bitcointApiClient) : base(payApiSettings, client, gnerateAddressClient, storeRequestClient, bitcointApiClient)
        {
        }

        [HttpGet("{assertId}/{nonempty}")]
        public async Task<IActionResult> GetNonEmpty(string assertId, string nonempty)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            if (string.IsNullOrEmpty(assertId))
            {
                return NotFound();
            }

            var result = await GetListOfSources(assertId);

            if (string.IsNullOrEmpty(nonempty) || !nonempty.Equals("nonempty"))
            {
                return Json(result);
            }

            return Json(from r in result
                where r.Amount > 0
                select r); 
        }

        [HttpGet("{assertId}")]
        public async Task<IActionResult> Get(string assertId)
        {
            return await GetNonEmpty(assertId, null);
        }

       

    }
}