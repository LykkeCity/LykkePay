using Lykke.AzureRepositories;
using Lykke.Core;

namespace LykkePay.API.Models
{
    public class TransferRequest : IStoreRequest
    {
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public string SuccessUrl { get; set; }
        public string ErrorUrl { get; set; }
        public string ProgressUrl { get; set; }
        public string OrderId { get; set; }

        public MerchantPayRequest GetRequest()
        {
            return new MerchantPayRequest
            {
                
                MerchantPayRequestStatus = MerchantPayRequestStatus.New,
                MerchantPayRequestType = MerchantPayRequestType.Transfer,
                MerchantPayRequestNotification = MerchantPayRequestNotification.Nothing,
                DestinationAddress = DestinationAddress,
                SourceAddress = SourceAddress,
                Amount = Amount,
                AssetId = AssetId,
                SuccessUrl = SuccessUrl,
                ErrorUrl = ErrorUrl,
                ProgressUrl = ProgressUrl,
                OrderId = OrderId
            };
        }
    }
}
