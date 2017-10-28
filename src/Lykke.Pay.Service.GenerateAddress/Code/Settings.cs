namespace Lykke.Pay.Service.GenerateAddress.Code
{
    public class Settings
    {
        public PayServiceGenAddressSettings PayServiceGenAddress { get; set; }

    }
    public class PayServiceGenAddressSettings
    {
        public DbSettings Db { get; set; }
        public string DataEncriptionPassword { get; set; }
        public int GenerateKeySize { get; set; }
        public Services Services { get; set; }
    }

    public class Services
    {
        public string  BitcoinApiUrl { get; set; }
    }


    public class DbSettings
    {
        public string PrivateKeysConnString { get; set; }

    }

}
