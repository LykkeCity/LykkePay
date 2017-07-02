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
    public class TransferController : BaseController
    {
        private readonly ILykkePayServiceStoreRequestMicroService _storeRequestClient;

        private readonly IBitcoinApi _bitcointApiClient;

        private readonly ILykkePayServiceGenerateAddressMicroService _generateAddressClient;

        public TransferController(PayApiSettings payApiSettings, HttpClient client, ILykkePayServiceStoreRequestMicroService storeRequestClient, IBitcoinApi bitcointApiClient, ILykkePayServiceGenerateAddressMicroService generateAddressClient) : base(payApiSettings, client)
        {
            _storeRequestClient = storeRequestClient;
            _bitcointApiClient = bitcointApiClient;
            _generateAddressClient = generateAddressClient;
        }

        private async Task<List<ToOneAddress>> GetListOfSources(TransferRequest request)
        {
            if (string.IsNullOrEmpty(request.AssetId))
            {
                return null;
            }

            var walletResult = await _generateAddressClient.ApiWalletByMerchantIdGetWithHttpMessagesAsync(MerchantId);
            return (from w in walletResult.Body
                    where request.AssetId.Equals(w.Assert, StringComparison.CurrentCultureIgnoreCase)
                    select new ToOneAddress
                    {
                        Address = w.WalletAddress,
                        Amount = (decimal?)w.Amount
                    }).ToList();
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

            var result = new TransferInProgressReturn
            {
                TransferResponse = new TransferInProgressResponse
                {
                    Settlement = Settlement.TRANSACTION_DETECTED,
                    TimeStamp = DateTime.UtcNow.Ticks,
                    Currency = request.AssetId
                }
            };
            try
            {
                var store = request.GetRequest();
                store.MerchantId = MerchantId;

                var list = await GetListOfSources(request);
                if (list == null || list.Count == 0 || (!string.IsNullOrEmpty(store.SourceAddress) &&
                                                        list.FirstOrDefault(l => store.SourceAddress
                                                            .Equals(l.Address)) == null))
                {
                    return Json(
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.INVALID_ADDRESS,
                                TimeStamp = DateTime.UtcNow.Ticks
                            }
                        });
                }


                var sourceList = new List<ToOneAddress>();
                if (!string.IsNullOrEmpty(store.SourceAddress))
                {
                    sourceList.Add(list.First(l => store.SourceAddress
                        .Equals(l.Address)));
                }
                else
                {
                    sourceList.AddRange(list);
                }

                if (!store.Amount.HasValue || store.Amount.Value <= 0)
                {
                    return Json(
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.INVALID_AMOUNT,
                                TimeStamp = DateTime.UtcNow.Ticks
                            }
                        });
                }

                decimal amountToPay = (decimal)store.Amount.Value;
                foreach (var src in sourceList)
                {
                    if (!src.Amount.HasValue || src.Amount.Value == 0 || amountToPay == 0)
                    {
                        src.Amount = 0;
                        continue;
                    }

                    var amout = Math.Max(src.Amount.Value, amountToPay);
                    if (amout > amountToPay)
                    {
                        amountToPay = 0;
                        src.Amount = amountToPay;
                    }
                    else
                    {
                        amountToPay -= amout;
                    }

                }

                if (amountToPay > 0)
                {
                    return Json(
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.INVALID_AMOUNT,
                                TimeStamp = DateTime.UtcNow.Ticks
                            }
                        });
                }

                var mtRequest = new MultipleTransferRequest
                {
                    Asset = store.AssetId,
                    Destination = store.DestinationAddress,
                    Sources = new List<ToOneAddress>(from sl in sourceList
                                                     where sl.Amount.HasValue && sl.Amount > 0
                                                     select sl)

                };

                var r = await _bitcointApiClient.ApiEnqueueTransactionMultipletransferPostWithHttpMessagesAsync(mtRequest);
                var resData = r?.Body as TransactionIdResponse;
                if (resData?.TransactionId == null)
                {
                    return Json(
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.TRANSACTION_NOT_CONFIRMED,
                                TimeStamp = DateTime.UtcNow.Ticks
                            }
                        });
                }
                store.TransactionId = resData.TransactionId.Value.ToString();
                store.MerchantPayRequestNotification = MerchantPayRequestNotification.InProgress.ToString();
                await _storeRequestClient.ApiStorePostWithHttpMessagesAsync(store);
                result.TransferResponse.TransactionId = store.TransactionId;
            }
            catch (Exception e)
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


            return Json(result);
        }


    }



}
