namespace LykkePay.API.Code
{
    public class Settings
    {
        public PayApiSettings PayApi { get; set; }

    }
    public class PayApiSettings
    {
        public ServicesSettings Services { get; set; }
    }


    public class ServicesSettings
    {
        public string MerchantAuthService { get; set; }

        public string PayServiceService { get; set; }

        public string GenerateAddressService { get; set; }

    }
}
