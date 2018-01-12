using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Common.Log;
using Lykke.Contracts.Pay;
using Lykke.Core;
using Lykke.Pay.Common;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using Lykke.Pay.Service.Wallets.Client;
using Lykke.Service.ExchangeOperations.Client;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LykkePay.API.Controllers
{
    //[Produces("application/json")]
    [Route("api/v1/Purchase")]
    public class PurchaseController : BaseTransactionController
    {
        private readonly IExchangeOperationsServiceClient _exchangeOperationClient;
        

        public PurchaseController(
            PayApiSettings payApiSettings, 
            HttpClient client, 
            ILykkePayServiceStoreRequestMicroService storeRequestClient, 
            IBitcoinApi bitcointApiClient,
            ILykkePayServiceGenerateAddressMicroService generateAddressClient, 
            IExchangeOperationsServiceClient exchangeOperationClient, 
            IPayWalletservice payWalletservice, 
            ILog log)
            : base(payApiSettings, client, generateAddressClient, storeRequestClient, bitcointApiClient, payWalletservice, log)
        {
            _exchangeOperationClient = exchangeOperationClient;
        }

        [HttpPost]
        public async Task<IActionResult> Purchase([FromBody] PurchaseRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }
            var store = request.GetRequest();
            AssertPairRate rate;
            try
            {
                var rateServiceUrl = $"{PayApiSettings.Services.PayServiceService}?sessionId={MerchantSessionId}&cacheTimeout={Merchant?.TimeCacheRates}";

                var response = JsonConvert.DeserializeObject<AssertListWithSession>(
                    await (await HttpClient.GetAsync(rateServiceUrl)).Content
                        .ReadAsStringAsync());

                var newSessionId = response.SessionId;
                var rates = response.Asserts;

                if (!string.IsNullOrEmpty(MerchantSessionId) && !MerchantSessionId.Equals(newSessionId))
                {
                    throw new InvalidDataException("Session expired");
                }
                StoreNewSessionId(newSessionId);
                rate = rates.First(r => r.AssetPair.Equals(request.AssetPair, StringComparison.CurrentCultureIgnoreCase));
                if (rate == null)
                {
                    await Log.WriteWarningAsync(nameof(PurchaseController), nameof(Purchase), LogContextPayRequest(store), $"Not found rate for {request.AssetPair}, return internal error");
                    return await JsonAndStoreError(store,
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.INTERNAL_ERROR,
                                TimeStamp = DateTime.UtcNow.Ticks
                            }
                        });
                }
            }
            catch(Exception exception)
            {
                await Log.WriteWarningAsync(nameof(PurchaseController), nameof(Purchase), LogContextPayRequest(store), "Exception on get rate process", exception);

                return await JsonAndStoreError(store,
                    new TransferErrorReturn
                    {
                        TransferResponse = new TransferErrorResponse
                        {
                            TransferError = TransferError.INTERNAL_ERROR,
                            TimeStamp = DateTime.UtcNow.Ticks
                        }
                    });
            }

            dynamic pairReal;
            try
            {
                var respAssertPairList = await HttpClient.GetAsync(new Uri(PayApiSettings.Services.MarketProfileService));
                var sAssertPairList = await respAssertPairList.Content.ReadAsStringAsync();
                var pairsList = JsonConvert.DeserializeObject<List<dynamic>>(sAssertPairList);
                pairReal = pairsList.First(r => request.AssetPair.Equals((string)r.AssetPair, StringComparison.CurrentCultureIgnoreCase));
                if (pairReal == null)
                {
                    await Log.WriteWarningAsync(nameof(PurchaseController), nameof(Purchase), LogContextPayRequest(store), $"Not found asset pair {request.AssetPair}, return internal error");

                    return await JsonAndStoreError(store,
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.INTERNAL_ERROR,
                                TimeStamp = DateTime.UtcNow.Ticks
                            }
                        });
                }
            }
            catch(Exception exception)
            {
                await Log.WriteWarningAsync(nameof(PurchaseController), nameof(Purchase), LogContextPayRequest(store), "Exception on get pairReal process", exception);

                return await JsonAndStoreError(store,
                    new TransferErrorReturn
                    {
                        TransferResponse = new TransferErrorResponse
                        {
                            TransferError = TransferError.INTERNAL_ERROR,
                            TimeStamp = DateTime.UtcNow.Ticks
                        }
                    });
            }


            string merchantClientId = string.Empty;
            try
            {
                merchantClientId =
                    await (await HttpClient.GetAsync($"{PayApiSettings.Services.MerchantClientService}{MerchantId}"))
                        .Content
                        .ReadAsStringAsync();
            }
            catch (Exception e)
            {

            }
            if (string.IsNullOrEmpty(merchantClientId))
            {
                await Log.WriteWarningAsync(nameof(PurchaseController), nameof(Purchase), LogContextPayRequest(store), $"Not found merchantClientId for {MerchantId}");

                return await JsonAndStoreError(store,
                    new TransferErrorReturn
                    {
                        TransferResponse = new TransferErrorResponse
                        {
                            TransferError = TransferError.INTERNAL_ERROR,
                            TimeStamp = DateTime.UtcNow.Ticks
                        }
                    });
            }


            double realAmount;
            if (request.Markup != null)
            {
                double fee = (double)request.PaidAmount * (request.Markup.Percent / 100);
                fee += Math.Pow(10, -1 * rate.Accuracy) * request.Markup.Pips;
                fee += request.Markup.FixedFee;
                realAmount = (double)request.PaidAmount - fee;
            }
            else
            {
                realAmount = (double)request.PaidAmount;
            }

            double outSpread = rate.Ask + (rate.Ask - rate.Bid) * PayApiSettings.SpredK;
            double reakAsk = (double)((JObject)pairReal)["AskPrice"];
            double featToSell = realAmount * (reakAsk / outSpread);

            double btcToBuy = featToSell / reakAsk;

            store.Amount = btcToBuy;
            store.SourceAddress = PayApiSettings.HotWalletAddress;
            store.MerchantId = MerchantId;
            var post = await PostTransferRaw(BitcoinAssert, store);
            var result = post as IActionResult;
            if (result != null)
            {
                return result;
            }


            string fiatAssert = request.AssetPair.Replace(BitcoinAssert, string.Empty);
            //try
            //{

            var transResult = await _exchangeOperationClient.TrustedTransferAsync(PayApiSettings.LykkePayId, merchantClientId,
                realAmount, fiatAssert);
            //TODO Add logs here if wrong;
            var trageResult = await _exchangeOperationClient.TrustedTradeAsync(PayApiSettings.LykkePayId,
                request.AssetPair, featToSell, fiatAssert);
            //TODO Add logs here if wrong;
        //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw;
            //}
           



            return Json(post);


    }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetStatus(string id)
        {
            return await GetTransactionStatus(id);
        }

        [HttpPost("{id}/successUrl")]
        public async Task<IActionResult> UpdateSucecessUrl(string id, [FromBody] UrlRequest url)
        {
            var result = await UpdateUrl(id, url.Url, UrlType.Success);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500);
        }

        [HttpPost("{id}/progressUrl")]
        public async Task<IActionResult> UpdateProgressUrl(string id, [FromBody] UrlRequest url)
        {
            var result = await UpdateUrl(id, url.Url, UrlType.InProgress);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500);
        }

        [HttpPost("{id}/errorUrl")]
        public async Task<IActionResult> UpdateErrorUrl(string id, [FromBody] UrlRequest url)
        {
            var result = await UpdateUrl(id, url.Url, UrlType.Error);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500);
        }
    }
}