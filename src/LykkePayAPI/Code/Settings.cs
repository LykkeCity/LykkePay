namespace LykkePay.API.Code
{
    public class Settings
    {
        public PayApiSettings PayApi { get; set; }

    }
    public class PayApiSettings
    {
        public ServicesSettings Services { get; set; }
        public string HotWalletAddress { get; set; }
        public string LykkePayId { get; set; }
        public double SpredK { get; set; }
    }


    public class ServicesSettings
    {
        public string MerchantAuthService { get; set; }

        public string PayServiceService { get; set; }

        public string GenerateAddressService { get; set; }

        public string StoreRequestService { get; set; }

        public string BitcoinApi { get; set; }

        public string MarketProfileService { get; set; }
        public string ExchangeOperationsService { get; internal set; }
        public string MerchantClientService { get; set; }
    }
}
