using Lykke.AzureRepositories;
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


        public virtual MerchantPayRequest GetRequest()
        {
            return new MerchantPayRequest
            {
                Markup = new PayFee
                {
                    FixedFee = Markup.FixedFee,
                    Percent = Markup.Percent,
                    Pips = Markup.Pips
                },
                MerchantPayRequestStatus = MerchantPayRequestStatus.New,
                MerchantPayRequestType = MerchantPayRequestType.Purchase,
                MerchantPayRequestNotification = MerchantPayRequestNotification.Nothing,
                DestinationAddress = DestinationAddress,
                AssetPair = AssetPair,
                Amount = PaidAmount,
                AssetId = BaseAsset,
                SuccessUrl = SuccessUrl,
                ErrorUrl = ErrorUrl,
                ProgressUrl = ProgressUrl,
                OrderId = OrderId
            };
        }
    }
}
