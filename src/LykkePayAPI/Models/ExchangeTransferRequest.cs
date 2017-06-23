
namespace LykkePay.API.Models
{
    public class ExchangeTransferRequest : PurchaseRequest
    {
        public string SourceAddress { get; set; }

        public override Lykke.Pay.Service.StoreRequest.Client.Models.IMerchantPayRequest GetRequest()
        {
            var result = base.GetRequest();
            result.SourceAddress = SourceAddress;
            return result;
        }

    }
}
