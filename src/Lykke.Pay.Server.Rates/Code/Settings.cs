namespace Lykke.Pay.Service.Rates.Code
{
    public class Settings
    {
        public PayServiceRatesSettings PayServiceRates { get; set; }
        public MarketProfileServiceClientSettings MarketProfileServiceClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }

    }
    public class PayServiceRatesSettings
    {
        public DbSettings Db { get; set; }
        public int CacheTimeout { get; set; }
       
    }


    public class DbSettings
    {
        public string AssertHistoryConnString { get; set; }

    }
    
    public class AssetsServiceClientSettings
    {
        public string ServiceUrl { get; set; }
    }

    public class MarketProfileServiceClientSettings
    {
        public string ServiceUrl { get; set; }
    }
}
