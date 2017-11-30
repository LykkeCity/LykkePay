using System;
using System.Globalization;
using Lykke.AzureRepositories.Extentions;
using Lykke.Core;
using PayFee = Lykke.Pay.Service.StoreRequest.Client.Models.PayFee;

namespace LykkePay.API.Models
{
    public class OrderRequest
    {
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string ExchangeCurrency { get; set; }
        public string SuccessUrl { get; set; }
        public string ErrorUrl { get; set; }
        public string ProgressUrl { get; set; }
        public string OrderId { get; set; }
        public Markup Markup { get; set; }

        public Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest GetRequest(string merchantId)
        {
            try
            {
                var provider = CultureInfo.InvariantCulture;
                return new Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest
                {
                    MerchantId = merchantId,
                    Amount = double.Parse(Amount, provider),
                    AssetPair = $"{ExchangeCurrency}{Currency}",
                    AssetId = Currency,
                    ExchangeAssetId = ExchangeCurrency,
                    OrderId = OrderId,
                    Markup = Markup == null ? null : new PayFee
                    {
                        FixedFee = Markup.FixedFee,
                        Percent = Markup.Percent,
                        Pips = Markup.Pips
                    },
                    SuccessUrl = SuccessUrl,
                    ErrorUrl = ErrorUrl,
                    ProgressUrl = ProgressUrl,
                    MerchantPayRequestNotification = MerchantPayRequestNotification.Nothing.ToString(),
                    MerchantPayRequestStatus = MerchantPayRequestStatus.New.ToString(),
                    RequestId = Guid.NewGuid().ToString(),
                    TransactionDetectionTime = DateTime.Now.StorageString(),
                    TransactionWaitingTime = DateTime.Now.AddMinutes(10).StorageString()

                };

            }
            catch
            {
                return null;
            }

            
        }
    }
}