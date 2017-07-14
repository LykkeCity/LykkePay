using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Lykke.Common.Entities.Pay;
using Lykke.Pay.Common;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LykkePay.API.Controllers
{
    [Route("api/ExchangeTransfer")]
    public class ExchangeTransferController : BaseTransactionController
    {

        public ExchangeTransferController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient,IBitcoinApi bitcointApiClient, ILykkePayServiceGenerateAddressMicroService generateAddressClient) 
            : base(payApiSettings, client, generateAddressClient, storeRequestClient, bitcointApiClient)
        {
            
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ExchangeTransferRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            var rates = JsonConvert.DeserializeObject<List<AssertPairRate>>(
                await (await HttpClient.GetAsync(PayApiSettings.Services.PayServiceService)).Content
                    .ReadAsStringAsync());

            var rate = rates.FirstOrDefault(r => r.AssetPair.Equals(request.AssetPair, StringComparison.CurrentCultureIgnoreCase));
            if (rate == null)
            {
                return Json(
                    new TransferErrorReturn
                    {
                        TransferResponse = new TransferErrorResponse
                        {
                            TransferError = TransferError.INTERNAL_ERROR,
                            TimeStamp = DateTime.UtcNow.Ticks
                        }
                    });
            }

            return Json(rate);

            var store = request.GetRequest();
            store.MerchantId = MerchantId;
            var json = JsonConvert.SerializeObject(store);
            await StoreRequestClient.ApiStorePostWithHttpMessagesAsync(store);

            return Content(store.RequestId);
        }
    }
}