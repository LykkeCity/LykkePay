namespace LykkePay.API.Models
{
    public class TransferRequest
    {
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string Amount { get; set; }
        public string AssetId { get; set; }
        public string SuccessUrl { get; set; }
        public string ErrorUrl { get; set; }
        public string ProgressUrl { get; set; }
        public string OrderId { get; set; }

    }
}
