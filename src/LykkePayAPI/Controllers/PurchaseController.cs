using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{
    //[Produces("application/json")]
    [Route("api/Purchase")]
    public class PurchaseController : BaseTransactionController
    {

        public PurchaseController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient, IBitcoinApi bitcointApiClient, ILykkePayServiceGenerateAddressMicroService generateAddressClient) 
            : base(payApiSettings, client, generateAddressClient, storeRequestClient, bitcointApiClient)
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

            var store = request.GetRequest();
            store.MerchantId = MerchantId;

            await StoreRequestClient.ApiStorePostAsync(store);

            return Content(store.RequestId);

            
        }
    }
}