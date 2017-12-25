using Lykke.Core;

namespace Lykke.Pay.Service.StoreRequest.Models
{
    public class OrderRequest : IMerchantOrderRequest
    {
        public string MerchantId { get; set; }
        public string RequestId { get; set; }
        public string TransactionId { get; set; }
        public PayFee Markup { get; set; }
        public MerchantPayRequestStatus MerchantPayRequestStatus { get; set; }
        public MerchantPayRequestNotification MerchantPayRequestNotification { get; set; }
        public string SourceAddress { get; set; }
        public string AssetPair { get; set; }
        public string ExchangeAssetId { get; set; }
        public double Amount { get; set; }
        public double OriginAmount { get; set; }
        public double ExchangeRate { get; set; }
        public string AssetId { get; set; }
        public string SuccessUrl { get; set; }
        public string ErrorUrl { get; set; }
        public string ProgressUrl { get; set; }
        public string OrderId { get; set; }
        public string TransactionDetectionTime { get; set; }
        public string TransactionWaitingTime { get; set; }
        public string Transaction { get; set; }
        public string TransactionStatus { get; set; }
    }
}