using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Common;
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
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using GenerateAddressRequest = Lykke.Pay.Service.GenerateAddress.Client.Models.GenerateAddressRequest;

namespace LykkePay.API.Controllers
{

    [Route("api/v1/Order")]
    public class OrderController : BaseTransactionController
    {
        private readonly ILykkePayServiceGenerateAddressMicroService _gaService;
        private readonly ILykkePayServiceStoreRequestMicroService _storeRequestClient;

        public OrderController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient, IBitcoinApi bitcointApiClient,
            ILykkePayServiceGenerateAddressMicroService generateAddressClient, IPayWalletservice payWalletservice, ILog log)
            : base(payApiSettings, client, generateAddressClient, storeRequestClient, bitcointApiClient, payWalletservice, log)
        {

            _gaService = generateAddressClient;
            _storeRequestClient = storeRequestClient;
        }

        [HttpPost("ReCreate/{address}")]
        public async Task<IActionResult> ReCreate(string address)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            var order = await GetOrder(address);
            if (order != null)
            {
                await Log.WriteInfoAsync(nameof(OrderController), nameof(CreateNewOrder), OrderContext(MerchantId, order), $"ReCreated order by address {address}");
                return Json(order);
            }

            return NotFound();
        }

        //todo: что это за метод!!, почему он не гет, почему он не json возвращает?
        [HttpPost("Status/{address}")]
        public async Task<IActionResult> Status(string address)
        {

            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            if (string.IsNullOrEmpty(address))
            {
                return NotFound();
            }

            var storeResponse = await _storeRequestClient.ApiStoreOrderByMerchantIdGetWithHttpMessagesAsync(MerchantId);
            var content = await storeResponse.Response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                return NotFound();
            }

            var result = (from o in JsonConvert
                    .DeserializeObject<List<Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest>>(content)
                where
                address.Equals(o.SourceAddress)
                group o by o.MerchantPayRequestStatus
                into go
                select go.Key.ParsePayEnum<MerchantPayRequestStatus>()).ToList();

            if (result.Exists(st => st == MerchantPayRequestStatus.Failed))
            {
                return Content(MerchantPayRequestStatus.Failed.ToString());
            }

            if (result.Exists(st => st == MerchantPayRequestStatus.Completed))
            {
                return Content(MerchantPayRequestStatus.Completed.ToString());
            }

            if (result.Exists(st => st == MerchantPayRequestStatus.New))
            {
                return Content(MerchantPayRequestStatus.New.ToString());
            }

            return NotFound(); 

        }

        [HttpPost]
        public async Task<IActionResult> CreateNewOrder([FromBody] OrderRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }


            var store = request?.GetRequest(MerchantId);
            if (store == null)
            {
                return BadRequest();
            }



            store.MerchantPayRequestStatus = MerchantPayRequestStatus.New.ToString();
            //store.MerchantPayRequestNotification = MerchantPayRequestNotification.InProgress.ToString();

            var resp = await _gaService.ApiGeneratePostWithHttpMessagesAsync(new GenerateAddressRequest
            {
                MerchantId = MerchantId,
                AssertId = store.ExchangeAssetId
            });

            var result = await GenerateOrder(store, resp.Body.Address);
            if (result == null)
            {
                return BadRequest();
            }

            await Log.WriteInfoAsync(nameof(OrderController), nameof(CreateNewOrder), OrderContext(MerchantId, result), "Created new order");

            return Json(result);
        }

        private string OrderContext(string merchantId, OrderRequestResponse order)
        {
            var obj = new
            {
                MerchantId = merchantId,
                order.Timestamp,
                order.Address,
                order.OrderId,
                order.Currency,
                order.Amount,
                order.RecommendedFee,
                order.TotalAmount,
                order.ExchangeRate,
                order.OrderRequestId,
                order.TransactionWaitingTime,
                order.MerchantPayRequestStatus
            };

            return obj.ToJson();
        }

        private async Task<OrderRequestResponse> GenerateOrder(Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest store, string address)
        {

            store.SourceAddress = address;

            var result = await GetRate(store.AssetPair);

            var post = result as StatusCodeResult;
            if (post != null)
            {
                return null;
            }

            var rate = (AssertPairRateWithSession)result;

            var arpRequest = new AprRequest
            {
                Markup = new AssertPairRateRequest
                {
                    Percent = store.Markup.Percent ?? 0,
                    Pips = store.Markup.Pips ?? 0
                }

            };

            rate.Bid = CalculateValue(rate.Bid, rate.Accuracy, arpRequest, false);
            //rate.Ask = CalculateValue(rate.Ask, rate.Accuracy, arpRequest, true);
            store.OriginAmount = store.Amount;
            store.Amount = store.Amount / rate.Bid;
            await _storeRequestClient.ApiStoreOrderPostWithHttpMessagesAsync(store);



            return new OrderRequestResponse(store, rate.Bid);
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetStatus(string id)
        {
            return await GetOrderStatus(id);
        }



        [HttpPost("{id}/successUrl")]
        public async Task<IActionResult> UpdateSucecessUrl(string id, [FromBody] UrlRequest url)
        {
            var result = await UpdateOrderUrl(id, url.Url, UrlType.Success);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500);
        }

        [HttpPost("{id}/progressUrl")]
        public async Task<IActionResult> UpdateProgressUrl(string id, [FromBody] UrlRequest url)
        {
            var result = await UpdateOrderUrl(id, url.Url, UrlType.InProgress);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500);
        }

        [HttpPost("{id}/errorUrl")]
        public async Task<IActionResult> UpdateErrorUrl(string id, [FromBody] UrlRequest url)
        {
            var result = await UpdateOrderUrl(id, url.Url, UrlType.Error);
            if (result)
            {
                return Ok();
            }
            return StatusCode(500);
        }

        private async Task<bool> UpdateOrderUrl(string id, string url, UrlType urlType)
        {
            var order = await GetOrderById(id);
            if (order == null)
            {
                return false;
            }
            switch (urlType)
            {
                case UrlType.Success:
                    order.SuccessUrl = url;
                    break;
                case UrlType.Error:
                    order.ErrorUrl = url;
                    break;
                case UrlType.InProgress:
                    order.ProgressUrl = url;
                    break;
            }
            await _storeRequestClient.ApiStoreOrderPostWithHttpMessagesAsync(order);
            return true;
        }

        private async Task<OrderRequestResponse> GetOrder(string id)
        {
            var storeResponse = await _storeRequestClient.ApiStoreOrderByMerchantIdGetWithHttpMessagesAsync(MerchantId);
            var content = await storeResponse.Response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            var result = (from o in JsonConvert.DeserializeObject<List<Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest>>(content)
                where
                (id.Equals(o.RequestId, StringComparison.CurrentCultureIgnoreCase) || id.Equals(o.OrderId, StringComparison.CurrentCultureIgnoreCase) ||
                 !string.IsNullOrEmpty(o.TransactionId) && o.TransactionId.Equals(id, StringComparison.CurrentCultureIgnoreCase) ||
                 !string.IsNullOrEmpty(o.SourceAddress) && o.SourceAddress.Equals(id, StringComparison.CurrentCultureIgnoreCase))
                && !string.IsNullOrEmpty(o.TransactionWaitingTime)
                orderby o.TransactionWaitingTime.GetRepoDateTime()
                select o).ToList();
            if (result.Count == 0)
            {
                return null;
            }
            if (result.Count == 1 && result[0].TransactionWaitingTime.GetRepoDateTime() > DateTime.Now && result[0].MerchantPayRequestStatus != ((int)MerchantPayRequestStatus.New).ToString())
            {
                return new OrderRequestResponse(result[0]);
            }

            var order = result.FirstOrDefault(
                o => o.MerchantPayRequestStatus != ((int)MerchantPayRequestStatus.New).ToString());
            if (order != null)
            {
                return new OrderRequestResponse(order);
            }


            order = result.FirstOrDefault(o => o.TransactionWaitingTime.GetRepoDateTime() > DateTime.Now);
            if (order != null)
            {
                return new OrderRequestResponse(order);
            }

            var oRequest = result.First();



            var request = new OrderRequest
            {
                Amount = oRequest.OriginAmount.ToString(),
                Currency = oRequest.AssetId,
                ExchangeCurrency = oRequest.ExchangeAssetId,
                ErrorUrl = oRequest.ErrorUrl,
                ProgressUrl = oRequest.ProgressUrl,
                SuccessUrl = oRequest.SuccessUrl,
                Markup = new Markup
                {
                    FixedFee = oRequest.Markup.FixedFee ?? 0,
                    Percent = oRequest.Markup.Percent ?? 0,
                    Pips = 0
                },
                OrderId = oRequest.OrderId,



            };

            var store = request.GetRequest(MerchantId);
            
            if (store == null)
            {
                return null;
            }

            store.OriginAmount = oRequest.OriginAmount;
            return await GenerateOrder(store, result.First().SourceAddress);
        }

        private async Task<Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest> GetOrderById(string id)
        {
            var storeResponse = await _storeRequestClient.ApiStoreOrderByMerchantIdGetWithHttpMessagesAsync(MerchantId);
            var content = await storeResponse.Response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            return (from o in JsonConvert.DeserializeObject<List<Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest>>(content)
                          where
                          (id.Equals(o.RequestId, StringComparison.CurrentCultureIgnoreCase) || id.Equals(o.OrderId, StringComparison.CurrentCultureIgnoreCase) ||
                          !string.IsNullOrEmpty(o.TransactionId) && o.TransactionId.Equals(id, StringComparison.CurrentCultureIgnoreCase) ||
                          !string.IsNullOrEmpty(o.SourceAddress) && o.SourceAddress.Equals(id, StringComparison.CurrentCultureIgnoreCase))
                          select o).FirstOrDefault();
           
        }

        private async Task<IActionResult> GetOrderStatus(string id)
        {
            var order = await GetOrderById(id);
            if (order == null)
            {
                return Json(new PaymentErrorReturn 
                {
                    PaymentResponse = new PaymentErrorResponse
                    {
                        PaymentError = PaymentError.TRANSACTION_NOT_DETECTED,
                        TimeStamp = DateTime.UtcNow.Ticks
                    },
                    PaymentStatus = PaymentStatus.PAYMENT_ERROR
                });
            }





            if (order.MerchantPayRequestStatus.Equals(MerchantPayRequestStatus.Completed.ToString()))
            {
                return Json(new PaymentSuccessReturn
                        {
                            PaymentResponse = new PaymentSuccessResponse
                            {
                                TransactionId = order.TransactionId,
                                Currency = order.AssetId,
                                NumberOfConfirmation = GetNumberOfConfirmation(order.SourceAddress, order.TransactionId),
                                TimeStamp = DateTime.UtcNow.Ticks,
                                Url = $"{PayApiSettings.LykkePayBaseUrl}transaction/{order.TransactionId}"
                            }
                        }
                    
                );
            }
            if (order.MerchantPayRequestStatus.Equals(MerchantPayRequestStatus.InProgress.ToString()))
            {
                return Json(new PaymentInProgressReturn
                {
                    PaymentResponse = new PaymentInProgressResponse
                    {
                        Settlement = Settlement.TRANSACTION_DETECTED,
                        TimeStamp = DateTime.UtcNow.Ticks,
                        Currency = order.AssetId,
                        TransactionId = order.TransactionId
                    }
                });
            }
            if (order.MerchantPayRequestStatus.Equals(MerchantPayRequestStatus.Failed.ToString()))
            {
                var transferStatus = string.IsNullOrEmpty(order.TransactionStatus)
                    ? InvoiceStatus.Unpaid
                    : order.TransactionStatus.ParsePayEnum<InvoiceStatus>();
                PaymentError paymentError;
                switch (transferStatus)
                {
                    case InvoiceStatus.Draft:
                    case InvoiceStatus.InProgress:
                    case InvoiceStatus.Paid:
                    case InvoiceStatus.Removed:
                        paymentError = PaymentError.TRANSACTION_NOT_DETECTED;
                        break;
                    case InvoiceStatus.LatePaid:
                    case InvoiceStatus.Unpaid:
                        paymentError = PaymentError.PAYMENT_EXPIRED;
                        break;
                    case InvoiceStatus.Overpaid:
                        paymentError = PaymentError.AMOUNT_ABOVE;
                        break;
                    case InvoiceStatus.Underpaid:
                        paymentError = PaymentError.AMOUNT_BELOW;
                        break;
                    default:
                        paymentError = PaymentError.TRANSACTION_NOT_DETECTED;
                        break;

                }

                return Json(new PaymentErrorReturn
                {
                    PaymentResponse = new PaymentErrorResponse
                    {
                        PaymentError = paymentError,
                        TimeStamp = DateTime.UtcNow.Ticks
                    }
                });
            }

            return Json(new PaymentErrorReturn
            {
                PaymentResponse = new PaymentErrorResponse
                {
                    PaymentError = PaymentError.TRANSACTION_NOT_DETECTED,
                    TimeStamp = DateTime.UtcNow.Ticks
                },
                PaymentStatus = PaymentStatus.PAYMENT_ERROR
            });
        }
    }
}