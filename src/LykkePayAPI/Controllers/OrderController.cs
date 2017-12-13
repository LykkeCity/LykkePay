using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Lykke.Contracts.Pay;
using Lykke.Core;
using Lykke.Pay.Common;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using Lykke.Service.ExchangeOperations.Client;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using GenerateAddressRequest = Lykke.Pay.Service.GenerateAddress.Client.Models.GenerateAddressRequest;

namespace LykkePay.API.Controllers
{

    [Route("api/Order")]
    public class OrderController : BaseTransactionController
    {
        private readonly ILykkePayServiceGenerateAddressMicroService _gaService;
        private readonly ILykkePayServiceStoreRequestMicroService _storeRequestClient;

        public OrderController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient, IBitcoinApi bitcointApiClient,
            ILykkePayServiceGenerateAddressMicroService generateAddressClient, IExchangeOperationsServiceClient exchangeOperationClient, IBitcoinAggRepository bitcoinAddRepository)
            : base(payApiSettings, client, generateAddressClient, storeRequestClient, bitcointApiClient, bitcoinAddRepository)
        {

            _gaService = generateAddressClient;
            _storeRequestClient = storeRequestClient;
        }

        [HttpPost("ReCreate/{OrderId}")]
        public async Task<IActionResult> ReCreate(string orderId)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            var order = await GetOrder(orderId);
            if (order == null)
            {
                return NotFound();
            }


            order.MerchantPayRequestStatus = MerchantPayRequestStatus.New.ToString();
            order.MerchantPayRequestNotification = MerchantPayRequestNotification.Nothing.ToString();

            await _storeRequestClient.ApiStoreOrderPostWithHttpMessagesAsync(order);

            var request = new OrderRequest
            {
                Amount = order.OriginAmount.ToString(),
                Currency = order.AssetId,
                ExchangeCurrency = order.ExchangeAssetId,
                ErrorUrl = order.ErrorUrl,
                ProgressUrl = order.ProgressUrl,
                SuccessUrl = order.SuccessUrl,
                Markup = new Markup
                {
                    FixedFee = order.Markup.FixedFee ?? 0,
                    Percent = order.Markup.Percent ?? 0,
                    Pips = 0
                },
                OrderId = order.OrderId,
                

            };

            var store = request.GetRequest(MerchantId);

            if (store == null)
            {
                return BadRequest();
            }

            store.MerchantPayRequestStatus = MerchantPayRequestStatus.InProgress.ToString();
            store.MerchantPayRequestNotification = MerchantPayRequestNotification.InProgress.ToString();

            return await GenerateOrder(store, order.SourceAddress);

        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderRequest request)
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

            store.MerchantPayRequestStatus = MerchantPayRequestStatus.InProgress.ToString();
            store.MerchantPayRequestNotification = MerchantPayRequestNotification.InProgress.ToString();

            var resp = await _gaService.ApiGeneratePostWithHttpMessagesAsync(new GenerateAddressRequest
            {
                MerchantId = MerchantId,
                AssertId = store.ExchangeAssetId
            });

            return await GenerateOrder(store, resp.Body.Address);

        }

        private async Task<IActionResult> GenerateOrder(Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest store,  string address)
        {

            store.SourceAddress = address;

            var result = await GetRate(store.AssetPair);

            var post = result as StatusCodeResult;
            if (post != null)
            {
                return post;
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

            //rate.Bid = (float)CalculateValue(rate.Bid, rate.Accuracy, arpRequest, false);
            rate.Ask = (float)CalculateValue(rate.Ask, rate.Accuracy, arpRequest, true);
            store.OriginAmount = store.Amount;
            store.Amount = store.Amount / rate.Ask;
            await _storeRequestClient.ApiStoreOrderPostWithHttpMessagesAsync(store);



            return Json(new OrderRequestResponse(store, rate.Ask));
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
            var order = await GetOrder(id);
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

        private async Task<Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest> GetOrder(string id)
        {
            IEnumerable<Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest> result;
            var storeResponse = await _storeRequestClient.ApiStoreOrderByMerchantIdGetWithHttpMessagesAsync(MerchantId);
            var content = storeResponse.Response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content.Result))
            {
                return null;
            }

            result = (from order in JsonConvert.DeserializeObject<List<Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest>>(content.Result)
                where
                id.Equals(order.RequestId, StringComparison.CurrentCultureIgnoreCase) || id.Equals(order.OrderId, StringComparison.CurrentCultureIgnoreCase) ||
                      !string.IsNullOrEmpty(order.TransactionId) && order.TransactionId.Equals(id, StringComparison.CurrentCultureIgnoreCase) ||
                      !string.IsNullOrEmpty(order.SourceAddress) && order.SourceAddress.Equals(id, StringComparison.CurrentCultureIgnoreCase)
                select order);
            return result.FirstOrDefault();
        }

        private async Task<IActionResult> GetOrderStatus(string id)
        {
            var order = await GetOrder(id);
            if (order == null)
            {
                return Json(new TransferErrorReturn
                {
                    TransferResponse = new TransferErrorResponse
                    {
                        TransferError = TransferError.INTERNAL_ERROR,
                        TimeStamp = DateTime.UtcNow.Ticks
                    },
                    TransferStatus = TransferStatus.TRANSFER_ERROR
                });
            }

            if (order.MerchantPayRequestStatus.Equals(MerchantPayRequestStatus.Completed.ToString()))
            {
                return Json(new TransferSuccessReturn
                    {
                        TransferResponse = new TransferSuccessResponse
                        {
                            TransactionId = order.TransactionId,
                            Currency = order.AssetId,
                            NumberOfConfirmation = await GetNumberOfConfirmation(order.SourceAddress, order.TransactionId),
                            TimeStamp = DateTime.UtcNow.Ticks,
                            Url = $"{PayApiSettings.LykkePayBaseUrl}transaction/{order.TransactionId}"
                        },
                        TransferStatus = TransferStatus.TRANSFER_CONFIRMED
                    }



                );
            }
            if (order.MerchantPayRequestStatus.Equals(MerchantPayRequestStatus.InProgress.ToString()))
            {
                return Json(new TransferInProgressReturn
                {
                    TransferResponse = new TransferInProgressResponse
                    {
                        Settlement = Settlement.TRANSACTION_DETECTED,
                        TimeStamp = DateTime.UtcNow.Ticks,
                        Currency = string.IsNullOrEmpty(order.AssetId) ? order.AssetPair : order.AssetId,
                        TransactionId = order.TransactionId
                    },
                    TransferStatus = TransferStatus.TRANSFER_INPROGRESS
                });
            }
            if (order.MerchantPayRequestStatus.Equals(MerchantPayRequestStatus.Failed.ToString()))
            {
                return Json(new TransferErrorReturn
                {
                    TransferResponse = new TransferErrorResponse
                    {
                        TransferError = Enum.Parse<TransferError>(order.MerchantPayRequestNotification),
                        TimeStamp = DateTime.UtcNow.Ticks
                    },
                    TransferStatus = TransferStatus.TRANSFER_ERROR
                });
            }

            return Json(new TransferErrorReturn
            {
                TransferResponse = new TransferErrorResponse
                {
                    TransferError = TransferError.INTERNAL_ERROR,
                    TimeStamp = DateTime.UtcNow.Ticks
                },
                TransferStatus = TransferStatus.TRANSFER_ERROR

            });
        }
    }
}