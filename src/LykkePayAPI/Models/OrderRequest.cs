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
    }
}