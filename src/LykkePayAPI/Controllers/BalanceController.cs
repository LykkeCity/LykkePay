using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Common.Log;
using Lykke.Core;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using Lykke.Pay.Service.Wallets.Client;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/getBalance")]
    public class BalanceController : BaseTransactionController
    {

        public BalanceController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceGenerateAddressMicroService gnerateAddressClient, ILykkePayServiceStoreRequestMicroService storeRequestClient,
            IBitcoinApi bitcointApiClient, IPayWalletservice payWalletservice, ILog log) 
            : base(payApiSettings, client, gnerateAddressClient, storeRequestClient, bitcointApiClient, payWalletservice, log)
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