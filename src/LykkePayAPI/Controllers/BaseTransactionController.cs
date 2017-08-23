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
using Lykke.Pay.Service.StoreRequest.Client.Models;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{
    public class BaseTransactionController : BaseController
    {
        protected readonly ILykkePayServiceGenerateAddressMicroService GnerateAddressClient;
        protected readonly ILykkePayServiceStoreRequestMicroService StoreRequestClient;
        protected readonly IBitcoinApi BitcointApiClient;

        

        public BaseTransactionController(PayApiSettings payApiSettings, HttpClient client,
            ILykkePayServiceGenerateAddressMicroService gnerateAddressClient,
            ILykkePayServiceStoreRequestMicroService storeRequestClient, IBitcoinApi bitcointApiClient) : base(payApiSettings, client)
        {
            GnerateAddressClient = gnerateAddressClient;
            StoreRequestClient = storeRequestClient;
            BitcointApiClient = bitcointApiClient;
        }

        protected async Task<JsonResult> JsonAndStoreError(PayRequest payRequest, TransferErrorReturn errorResponse)
        {
            payRequest.MerchantPayRequestNotification = MerchantPayRequestNotification.Error.ToString();
            await StoreRequestClient.ApiStorePostWithHttpMessagesAsync(payRequest);
            return Json(errorResponse);
        }

        protected async Task<IActionResult> PostTransfer(string assertId, PayRequest payRequest)
        {
            var store = payRequest;
            var result = new TransferInProgressReturn
            {
                TransferResponse = new TransferInProgressResponse
                {
                    Settlement = Settlement.TRANSACTION_DETECTED,
                    TimeStamp = DateTime.UtcNow.Ticks,
                    Currency = assertId
                }
            };
            try
            {
               
                store.MerchantId = MerchantId;
                store.MerchantPayRequestStatus = MerchantPayRequestStatus.InProgress.ToString();

                var list = await GetListOfSources(assertId);
                if (list == null || list.Count == 0 || !string.IsNullOrEmpty(store.SourceAddress) &&
                    list.FirstOrDefault(l => store.SourceAddress
                        .Equals(l.Address)) == null)
                {
                    return await JsonAndStoreError(store,
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
                    return await JsonAndStoreError(store,
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
                    return await JsonAndStoreError(store,
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
                    Asset = assertId,
                    Destination = store.DestinationAddress,
                    FeeRate = 0,
                    Sources = new List<ToOneAddress>(from sl in sourceList
                        where sl.Amount.HasValue && sl.Amount > 0
                        select sl)

                };

                var r = await BitcointApiClient.ApiTransactionMultipletransferPostWithHttpMessagesAsync(mtRequest);
                var resData = r?.Body as TransactionIdAndHashResponse;
                if (resData?.Hash == null)
                {
                    if (resData == null && "3".Equals((r?.Body as ApiException)?.Error.Code))
                    {
                        return await JsonAndStoreError(store,
                            new TransferErrorReturn
                            {
                                TransferResponse = new TransferErrorResponse
                                {
                                    TransferError = TransferError.INVALID_AMOUNT,
                                    TimeStamp = DateTime.UtcNow.Ticks
                                }
                            });
                    }
                    return await JsonAndStoreError(store,
                        new TransferErrorReturn
                        {
                            TransferResponse = new TransferErrorResponse
                            {
                                TransferError = TransferError.TRANSACTION_NOT_CONFIRMED,
                                TimeStamp = DateTime.UtcNow.Ticks
                            }
                        });
                }
                store.TransactionId = resData.Hash;
                store.MerchantPayRequestNotification = MerchantPayRequestNotification.InProgress.ToString();
                await StoreRequestClient.ApiStorePostWithHttpMessagesAsync(store);
                result.TransferResponse.TransactionId = store.TransactionId;
            }
            catch (Exception)
            {
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


            return Json(result);
        }

        public async Task<List<ToOneAddress>> GetListOfSources(string assertId)
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
            //var ww = res.Where(www => www.Address == "mqiPrLxjrPd8ihF67Fo5zqVCD4kvSoG16P").FirstOrDefault();
            return res;
        }
    }
}
