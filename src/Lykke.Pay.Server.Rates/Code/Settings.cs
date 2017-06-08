namespace Lykke.Pay.Service.Rates.Code
{
    public class Settings
    {
        public PayServiceRatesSettings PayServiceRates { get; set; }
        
    }
    public class PayServiceRatesSettings
    {
        public ServicesSettings Services { get; set; }
        public DbSettings Db { get; set; }
        public int CacheTimeout { get; set; }
       
    }


    public class DbSettings
    {
        public string AssertHistoryConnString { get; set; }

    }

   
    public class ServicesSettings
    {
        public string MarketProfileService { get; set; }
        public string MarginTradingAssetService { get; set; }

    }
}
