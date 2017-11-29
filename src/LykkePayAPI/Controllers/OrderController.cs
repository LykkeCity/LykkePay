//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Bitcoint.Api.Client;
//using Lykke.Common.Entities.Pay;
//using Lykke.Core;
//using Lykke.Pay.Service.GenerateAddress.Client;
//using Lykke.Pay.Service.StoreRequest.Client;
//using Lykke.Service.ExchangeOperations.Client;
//using LykkePay.API.Code;
//using LykkePay.API.Models;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using GenerateAddressRequest = Lykke.Pay.Service.GenerateAddress.Client.Models.GenerateAddressRequest;

//namespace LykkePay.API.Controllers
//{

//    [Route("api/Order")]
//    public class OrderController : BaseTransactionController
//    {
//        private readonly ILykkePayServiceGenerateAddressMicroService _gaService;
//        private IExchangeOperationsServiceClient _exchangeOperationClient;
//        public OrderController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient, IBitcoinApi bitcointApiClient,
//            ILykkePayServiceGenerateAddressMicroService generateAddressClient, IExchangeOperationsServiceClient exchangeOperationClient, IBitcoinAggRepository bitcoinAddRepository)
//            : base(payApiSettings, client, generateAddressClient, storeRequestClient, bitcointApiClient, bitcoinAddRepository)
//        {

//            _gaService = generateAddressClient;
//            _exchangeOperationClient = exchangeOperationClient;
//        }


//        [HttpPost]
//        public async Task<IActionResult> Post([FromBody] OrderRequest request)
//        {
//            var isValid = await ValidateRequest();
//            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
//            {
//                return isValid;
//            }
//            var store = request.GetRequest();

//            var resp = await _gaService.ApiGeneratePostWithHttpMessagesAsync(new GenerateAddressRequest
//            {
//                MerchantId = MerchantId,
//                AssertId = store.AssetId
//            });


//            store.DestinationAddress = resp.Body.Address;


//            AssertPairRate rate;
//            try
//            {
//                var rateServiceUrl = $"{PayApiSettings.Services.PayServiceService}?sessionId={MerchantSessionId}&cacheTimeout={Merchant?.TimeCacheRates}";

//                var response = JsonConvert.DeserializeObject<AssertListWithSession>(
//                    await (await HttpClient.GetAsync(rateServiceUrl)).Content
//                        .ReadAsStringAsync());

//                var newSessionId = response.SessionId;
//                var rates = response.Asserts;

//                if (!string.IsNullOrEmpty(MerchantSessionId) && !MerchantSessionId.Equals(newSessionId))
//                {
//                    throw new InvalidDataException("Session expired");
//                }
//                StoreNewSessionId(newSessionId);
//                rate = rates.First(r => r.AssetPair.Equals(store.AssetPair, StringComparison.CurrentCultureIgnoreCase));
//                if (rate == null)
//                {
//                    return await JsonAndStoreError(store,
//                        new TransferErrorReturn
//                        {
//                            TransferResponse = new TransferErrorResponse
//                            {
//                                TransferError = TransferError.INTERNAL_ERROR,
//                                TimeStamp = DateTime.UtcNow.Ticks
//                            }
//                        });
//                }
//            }
//            catch
//            {
//                return await JsonAndStoreError(store,
//                    new TransferErrorReturn
//                    {
//                        TransferResponse = new TransferErrorResponse
//                        {
//                            TransferError = TransferError.INTERNAL_ERROR,
//                            TimeStamp = DateTime.UtcNow.Ticks
//                        }
//                    });
//            }

//            dynamic pairReal;
//            try
//            {
//                var respAssertPairList = await HttpClient.GetAsync(new Uri(PayApiSettings.Services.MarketProfileService));
//                var sAssertPairList = await respAssertPairList.Content.ReadAsStringAsync();
//                var pairsList = JsonConvert.DeserializeObject<List<dynamic>>(sAssertPairList);
//                pairReal = pairsList.First(r => store.AssetPair.Equals((string)r.AssetPair, StringComparison.CurrentCultureIgnoreCase));
//                if (pairReal == null)
//                {
//                    return await JsonAndStoreError(store,
//                        new TransferErrorReturn
//                        {
//                            TransferResponse = new TransferErrorResponse
//                            {
//                                TransferError = TransferError.INTERNAL_ERROR,
//                                TimeStamp = DateTime.UtcNow.Ticks
//                            }
//                        });
//                }
//            }
//            catch
//            {
//                return await JsonAndStoreError(store,
//                    new TransferErrorReturn
//                    {
//                        TransferResponse = new TransferErrorResponse
//                        {
//                            TransferError = TransferError.INTERNAL_ERROR,
//                            TimeStamp = DateTime.UtcNow.Ticks
//                        }
//                    });
//            }


//            string merchantClientId = string.Empty;
//            try
//            {
//                merchantClientId =
//                    await (await HttpClient.GetAsync($"{PayApiSettings.Services.MerchantClientService}{MerchantId}"))
//                        .Content
//                        .ReadAsStringAsync();
//            }
//            catch
//            {

//            }
//            if (string.IsNullOrEmpty(merchantClientId))
//            {
//                return await JsonAndStoreError(store,
//                    new TransferErrorReturn
//                    {
//                        TransferResponse = new TransferErrorResponse
//                        {
//                            TransferError = TransferError.INTERNAL_ERROR,
//                            TimeStamp = DateTime.UtcNow.Ticks
//                        }
//                    });
//            }


//            double realAmount;
//            if (store.Markup != null)
//            {
//                double fee = (store.Amount ?? 0) * ((store.Markup.Percent ?? 0) / 100);
//                fee += Math.Pow(10, -1 * rate.Accuracy) * store.Markup.Pips ?? 0;
//                fee += store.Markup.FixedFee ?? 0;
//                realAmount = store.Amount ?? 0 - fee;
//            }
//            else
//            {
//                realAmount = store.Amount ?? 0;
//            }

//            double outSpread = rate.Ask + (rate.Ask - rate.Bid) * PayApiSettings.SpredK;
//            double reakAsk = (double)((JObject)pairReal)["AskPrice"];
//            double featToSell = realAmount * (reakAsk / outSpread);

//            double btcToBuy = featToSell / reakAsk;

//            store.Amount = btcToBuy;
//            store.SourceAddress = PayApiSettings.HotWalletAddress;
//            store.MerchantId = MerchantId;
//            var post = await PostTransferRaw(BitcoinAssert, store);
//            var result = post as IActionResult;
//            if (result != null)
//            {
//                return result;
//            }


//            string fiatAssert = store.AssetPair.Replace(BitcoinAssert, string.Empty);
//            //try
//            //{

//            var transResult = await _exchangeOperationClient.TrustedTransferAsync(PayApiSettings.LykkePayId, merchantClientId,
//                realAmount, fiatAssert);
//            //TODO Add logs here if wrong;
//            var trageResult = await _exchangeOperationClient.TrustedTradeAsync(PayApiSettings.LykkePayId,
//                store.AssetPair, featToSell, fiatAssert);
//            //TODO Add logs here if wrong;
//            //}
//            //catch (Exception e)
//            //{
//            //    Console.WriteLine(e);
//            //    throw;
//            //}




//            return Json(post);

//            throw new NotImplementedException();


//        }

//        [HttpGet("{id}/status")]
//        public async Task<IActionResult> GetStatus(string id)
//        {
//            return await GetTransactionStatus(id);
//        }

//        [HttpPost("{id}/successUrl")]
//        public async Task<IActionResult> UpdateSucecessUrl(string id, [FromBody] UrlRequest url)
//        {
//            var result = await UpdateUrl(id, url.Url, UrlType.Success);
//            if (result)
//            {
//                return Ok();
//            }
//            return StatusCode(500);
//        }

//        [HttpPost("{id}/progressUrl")]
//        public async Task<IActionResult> UpdateProgressUrl(string id, [FromBody] UrlRequest url)
//        {
//            var result = await UpdateUrl(id, url.Url, UrlType.InProgress);
//            if (result)
//            {
//                return Ok();
//            }
//            return StatusCode(500);
//        }

//        [HttpPost("{id}/errorUrl")]
//        public async Task<IActionResult> UpdateErrorUrl(string id, [FromBody] UrlRequest url)
//        {
//            var result = await UpdateUrl(id, url.Url, UrlType.Error);
//            if (result)
//            {
//                return Ok();
//            }
//            return StatusCode(500);
//        }
//    }
//}