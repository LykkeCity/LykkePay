
using System;
using Lykke.Core;

namespace LykkePay.API.Models
{
    public class PurchaseRequest : IStoreRequest
    {

        public string DestinationAddress { get; set; }

        public string AssetPair { get; set; }

        public string BaseAsset { get; set; }

        public decimal PaidAmount { get; set; }

        public string SuccessUrl { get; set; }

        public string ErrorUrl { get; set; }

        public string ProgressUrl { get; set; }

        public string OrderId { get; set; }

        public Markup Markup { get; set; }


        public virtual Lykke.Pay.Service.StoreRequest.Client.Models.PayRequest GetRequest()
        {
            return new Lykke.Pay.Service.StoreRequest.Client.Models.PayRequest
            {
                Markup = new Lykke.Pay.Service.StoreRequest.Client.Models.PayFee
                {
                    FixedFee = Markup.FixedFee,
                    Percent = Markup.Percent,
                    Pips = Markup.Pips
                },
                MerchantPayRequestStatus = MerchantPayRequestStatus.New.ToString(),
                MerchantPayRequestType = MerchantPayRequestType.Purchase.ToString(),
                MerchantPayRequestNotification = MerchantPayRequestNotification.Nothing.ToString(),
                DestinationAddress = DestinationAddress,
                AssetPair = AssetPair,
                Amount = (double)Math.Round(PaidAmount, 8),
                AssetId = BaseAsset,
                SuccessUrl = SuccessUrl,
                ErrorUrl = ErrorUrl,
                ProgressUrl = ProgressUrl,
                OrderId = OrderId,
                RequestId = Guid.NewGuid().ToString()
            };
        }
    }
}
