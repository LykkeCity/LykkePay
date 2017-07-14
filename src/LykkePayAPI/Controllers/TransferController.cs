using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Bitcoint.Api.Client.Models;
using Lykke.Core;
using Lykke.Pay.Common;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Mvc;
using TransferRequest = LykkePay.API.Models.TransferRequest;


namespace LykkePay.API.Controllers
{
    [Route("api/Transfer")]
    public class TransferController : BaseTransactionController
    {
      

        public TransferController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient, IBitcoinApi bitcointApiClient, ILykkePayServiceGenerateAddressMicroService generateAddressClient) 
            : base(payApiSettings, client, generateAddressClient, storeRequestClient, bitcointApiClient)
        {

        }

        

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TransferRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            return await PostTransfer(request.AssetId, request.GetRequest());

        }


    }



}
