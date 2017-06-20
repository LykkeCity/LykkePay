namespace LykkePay.API.Models
{
    public class PurchaseRequest
    {

        public string DestinationAddress { get; set; }

        public string AssetPair { get; set; }

        public string BaseAsset { get; set; }

        public string PaidAmount { get; set; }

        public string SuccessUrl { get; set; }

        public string ErrorUrl { get; set; }

        public string ProgressUrl { get; set; }
        public string OrderId { get; set; }

        public Markup Markup { get; set; }
       


    }
}
