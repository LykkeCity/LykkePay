using Lykke.AzureRepositories;

namespace LykkePay.API.Models
{
    public class ExchangeTransferRequest : PurchaseRequest
    {
        public string SourceAddress { get; set; }

        public override MerchantPayRequest GetRequest()
        {
            var result = base.GetRequest();
            result.SourceAddress = SourceAddress;
            return result;
        }

    }
}
