using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Bitcoint.Api.Client.Models;
using Common;
using Common.Log;
using Lykke.Core;
using Lykke.Pay.Common;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using Lykke.Pay.Service.StoreRequest.Client.Models;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LykkePay.API.Controllers
{
    public class BaseTransactionController : BaseController
    {
        protected const string BitcoinAssert = "BTC";
        protected enum UrlType
        {
            Success,
            InProgress,
            Error
        }

        protected readonly ILykkePayServiceGenerateAddressMicroService GnerateAddressClient;
        protected readonly ILykkePayServiceStoreRequestMicroService StoreRequestClient;
        protected readonly IBitcoinApi BitcointApiClient;
        protected readonly ILog Log;


        public BaseTransactionController(PayApiSettings payApiSettings, HttpClient client,
            ILykkePayServiceGenerateAddressMicroService gnerateAddressClient,
            ILykkePayServiceStoreRequestMicroService storeRequestClient, 
            IBitcoinApi bitcointApiClient,
            ILog log) 
                : base(payApiSettings, client)
        {
            GnerateAddressClient = gnerateAddressClient;
            StoreRequestClient = storeRequestClient;
            BitcointApiClient = bitcointApiClient;
            Log = log;
        }

        protected async Task<JsonResult> JsonAndStoreError(PayRequest payRequest, TransferErrorReturn errorResponse)
        {
            payRequest.MerchantPayRequestNotification = MerchantPayRequestNotification.Error.ToString();
            await StoreRequestClient.ApiStorePostWithHttpMessagesAsync(payRequest);
            return Json(errorResponse);
        }

        protected async Task<dynamic> PostTransferRaw(string assertId, PayRequest payRequest, int feeRate = 0)
        {
            var store = payRequest;
            var result = new TransferInProgressReturn
            {
                TransferResponse = new TransferInProgressResponse
                {
                    Settlement = Settlement.TRANSACTION_DETECTED,
                    TimeStamp = DateTime.UtcNow.Ticks,
                    Currency = assertId
                },
                TransferRequestId = payRequest.RequestId
            };
            try
            {

                store.MerchantId = MerchantId;
                store.MerchantPayRequestStatus = MerchantPayRequestStatus.InProgress.ToString();

                var list = await GetListOfSources(assertId) ?? new List<ToOneAddress>();
                if (store.SourceAddress == PayApiSettings.HotWalletAddress)
                    list.Add(new ToOneAddress(PayApiSettings.HotWalletAddress, (decimal)store.Amount * 2));

                if (!string.IsNullOrEmpty(store.SourceAddress) &&
                    list.FirstOrDefault(l => store.SourceAddress
                        .Equals(l.Address)) == null)
                {
                    await Log.WriteWarningAsync(nameof(PurchaseController), nameof(PostTransferRaw), LogContextPayRequest(store), $"Invalid adresses");

                    return await JsonAndStoreError(store,
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.INVALID_ADDRESS,
                                TimeStamp = DateTime.UtcNow.Ticks
                            },
                            TransferRequestId = payRequest.RequestId
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
                    await Log.WriteWarningAsync(nameof(PurchaseController), nameof(PostTransferRaw), LogContextPayRequest(store), $"Invalid amount");
                    return await JsonAndStoreError(store,
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.INVALID_AMOUNT,
                                TimeStamp = DateTime.UtcNow.Ticks
                            },
                            TransferRequestId = payRequest.RequestId
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
                        src.Amount = amountToPay;
                        amountToPay = 0;

                    }
                    else
                    {
                        amountToPay -= src.Amount.Value;
                    }

                }

                if (amountToPay > 0)
                {
                    await Log.WriteWarningAsync(nameof(PurchaseController), nameof(PostTransferRaw), LogContextPayRequest(store), $"Invalid amount, there is not enough money in the wallet for translation: {amountToPay}");

                    return await JsonAndStoreError(store,
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.INVALID_AMOUNT,
                                TimeStamp = DateTime.UtcNow.Ticks
                            },
                            TransferRequestId = payRequest.RequestId
                        });
                }

                var mtRequest = new MultipleTransferRequest
                {
                    Asset = assertId,
                    Destination = store.DestinationAddress,
                    FeeRate = feeRate,
                    Sources = new List<ToOneAddress>(from sl in sourceList
                                                     where sl.Amount.HasValue && sl.Amount > 0
                                                     select sl)

                };

                var r = await BitcointApiClient.ApiTransactionMultipletransferPostWithHttpMessagesAsync(mtRequest);
                var resData = r?.Body as TransactionIdAndHashResponse;
                if (resData?.Hash == null)
                {
                    var errorCode = (r?.Body as ApiException)?.Error.Code;
                    var errorMessage = (r?.Body as ApiException)?.Error.Message;
                    if (resData == null && errorCode == "3")
                    {
                        await Log.WriteWarningAsync(nameof(PurchaseController), nameof(PostTransferRaw), LogContextPayRequest(store), $"Invalid amount. Error on TransactionMultipletransfer: {errorMessage} ({errorCode})");

                        return await JsonAndStoreError(store,
                            new TransferErrorReturn
                            {
                                TransferResponse = new TransferErrorResponse
                                {
                                    TransferError = TransferError.INVALID_AMOUNT,
                                    TimeStamp = DateTime.UtcNow.Ticks
                                },
                                TransferRequestId = payRequest.RequestId
                            });
                    }

                    await Log.WriteWarningAsync(nameof(PurchaseController), nameof(PostTransferRaw), LogContextPayRequest(store), "Transaction not confirmed.");

                    return await JsonAndStoreError(store,
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.TRANSACTION_NOT_CONFIRMED,
                                TimeStamp = DateTime.UtcNow.Ticks
                            },
                            TransferRequestId = payRequest.RequestId
                        });
                }
                store.TransactionId = resData.Hash;
                store.MerchantPayRequestNotification = MerchantPayRequestNotification.InProgress.ToString();
                await StoreRequestClient.ApiStorePostWithHttpMessagesAsync(store);
                result.TransferResponse.TransactionId = store.TransactionId;
            }
            catch (Exception exception)
            {
                await Log.WriteWarningAsync(nameof(PurchaseController), nameof(PostTransferRaw), LogContextPayRequest(store), "Internal error. Exception on transfer.", exception);

                return await JsonAndStoreError(store,
                    new TransferErrorReturn
                    {
                        TransferResponse = new TransferErrorResponse
                        {
                            TransferError = TransferError.INTERNAL_ERROR,
                            TimeStamp = DateTime.UtcNow.Ticks
                        },
                        TransferRequestId = payRequest.RequestId
                    });
            }

            return result;
        }
        protected async Task<IActionResult> PostTransfer(string assertId, PayRequest payRequest, int feeRate = 0)
        {
            if (payRequest == null)
            {
                return BadRequest();
            }
            var post = await PostTransferRaw(assertId, payRequest, feeRate);
            var result = post as IActionResult;
            if (result != null)
            {
                return result;
            }
            return Json(post);
        }

        protected async Task<List<ToOneAddress>> GetListOfSources(string assertId)
        {
            if (string.IsNullOrEmpty(assertId))
            {
                return null;
            }

            var walletResult = await GnerateAddressClient.ApiWalletByMerchantIdGetWithHttpMessagesAsync(MerchantId);
            var res = (from w in walletResult.Body
                       where assertId.Equals(w.Assert, StringComparison.CurrentCultureIgnoreCase)
                       select new ToOneAddress
                       {
                           Address = w.WalletAddress,
                           Amount = (decimal?)w.Amount
                       }).ToList();

            return res;
        }


        private async Task<PayRequest> GetStoreRequest(string id)
        {
            var result = new List<PayRequest>();
            var storeResponse = await StoreRequestClient.ApiStoreGetWithHttpMessagesAsync();
            var content = storeResponse.Response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content.Result))
            {
                return null;
            }

            result.AddRange(from pr in JsonConvert.DeserializeObject<List<PayRequest>>(content.Result)
                            where !string.IsNullOrEmpty(pr.TransactionId) && pr.TransactionId.Equals(id, StringComparison.CurrentCultureIgnoreCase) ||
                               !string.IsNullOrEmpty(pr.SourceAddress) && pr.SourceAddress.Equals(id, StringComparison.CurrentCultureIgnoreCase) ||
                               !string.IsNullOrEmpty(pr.DestinationAddress) && pr.DestinationAddress.Equals(id, StringComparison.CurrentCultureIgnoreCase)
                            select pr);
            return result.FirstOrDefault();

        }

        protected async Task<bool> UpdateUrl(string id, string url, UrlType urlType)
        {
            var payRequest = await GetStoreRequest(id);
            if (payRequest == null)
            {
                return false;
            }
            switch (urlType)
            {
                case UrlType.Success:
                    payRequest.SuccessUrl = url;
                    break;
                case UrlType.Error:
                    payRequest.ErrorUrl = url;
                    break;
                case UrlType.InProgress:
                    payRequest.ProgressUrl = url;
                    break;
            }
            await StoreRequestClient.ApiStorePostWithHttpMessagesAsync(payRequest);

            await Log.WriteInfoAsync(this.GetType().Name, $"{nameof(UpdateUrl)} - {urlType.ToString()}", LogContextPayRequest(payRequest), $"Update callback url for request by {id}");

            return true;
        }

        protected string LogContextPayRequest(PayRequest request)
        {
            if (request == null)
                return null;
            
            var obj = new
            {
                request.TransactionId,
                request.MerchantId,
                request.SourceAddress,
                request.DestinationAddress,
                request.OrderId,
                request.RequestId
            };

            return obj.ToJson();
        }

        protected int GetNumberOfConfirmation(string address, string transactionId)
        {
          
            return PayApiSettings.TransactionConfirmation;
        }

        protected async Task<IActionResult> GetTransactionStatus(string id)
        {
            var payRequest = await GetStoreRequest(id);
            if (payRequest == null)
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

            if (payRequest.MerchantPayRequestStatus.Equals(MerchantPayRequestStatus.Completed.ToString()))
            {
                return Json(new TransferSuccessReturn
                {
                    TransferResponse = new TransferSuccessResponse
                    {
                        TransactionId = payRequest.TransactionId,
                        Currency = payRequest.AssetId,
                        NumberOfConfirmation = GetNumberOfConfirmation(payRequest.DestinationAddress, payRequest.TransactionId),
                        TimeStamp = DateTime.UtcNow.Ticks,
                        Url = $"{PayApiSettings.LykkePayBaseUrl}transaction/{payRequest.TransactionId}"
                    },
                    TransferStatus = TransferStatus.TRANSFER_CONFIRMED
                }



                );
            }
            if (payRequest.MerchantPayRequestStatus.Equals(MerchantPayRequestStatus.InProgress.ToString()))
            {
                return Json(new TransferInProgressReturn
                {
                    TransferResponse = new TransferInProgressResponse
                    {
                        Settlement = Settlement.TRANSACTION_DETECTED,
                        TimeStamp = DateTime.UtcNow.Ticks,
                        Currency = string.IsNullOrEmpty(payRequest.AssetId) ? payRequest.AssetPair : payRequest.AssetId,
                        TransactionId = payRequest.TransactionId
                    },
                    TransferStatus = TransferStatus.TRANSFER_INPROGRESS
                });
            }
            if (payRequest.MerchantPayRequestStatus.Equals(MerchantPayRequestStatus.Failed.ToString()))
            {
                return Json(new TransferErrorReturn
                {
                    TransferResponse = new TransferErrorResponse
                    {
                        TransferError = Enum.Parse<TransferError>(payRequest.MerchantPayRequestNotification),
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
