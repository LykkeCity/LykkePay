using System;
using Lykke.Core;
using Lykke.Pay.Service.StoreRequest.Client.Models;

namespace LykkePay.API.Models
{
    public class OrderRequest : IStoreRequest
    {

        public string Currency { get; set; }

        public double Amount { get; set; }

        public string ExchangeCurrency { get; set; }

        public string SuccessUrl { get; set; }

        public string ErrorUrl { get; set; }

        public string ProgressUrl { get; set; }

        public string OrderId { get; set; }

        public string PaymentTimeout { get; set; }

        public float Percent { get; set; }

        public int Pips { get; set; }

        public float FixedFee { get; set; }



        public PayRequest GetRequest()
        {
            return new Lykke.Pay.Service.StoreRequest.Client.Models.PayRequest
            {
                Markup = new Lykke.Pay.Service.StoreRequest.Client.Models.PayFee
                {
                    FixedFee = FixedFee,
                    Percent = Percent,
                    Pips = Pips
                },
                MerchantPayRequestStatus = MerchantPayRequestStatus.New.ToString(),
                MerchantPayRequestType = MerchantPayRequestType.Purchase.ToString(),
                MerchantPayRequestNotification = MerchantPayRequestNotification.Nothing.ToString(),
                AssetPair = $"{ExchangeCurrency}{Currency}",
                Amount = Amount,
                AssetId = ExchangeCurrency,
                SuccessUrl = SuccessUrl,
                ErrorUrl = ErrorUrl,
                ProgressUrl = ProgressUrl,
                OrderId = OrderId,
                RequestId = Guid.NewGuid().ToString()
            };
        }
    }
}