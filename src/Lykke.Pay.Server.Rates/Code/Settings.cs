namespace Lykke.Pay.Service.Rates.Code
{
    public class Settings
    {
        public PayServiceRatesSettings PayServiceRates { get; set; }
        
    }
    public class PayServiceRatesSettings
    {
        public ServicesSettings Services { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public int CacheTimeout { get; set; }
        public int RabbitDelay { get; set; }
        public int AccessCrossCount { get; set; }
    }


    public class RabbitMqSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string QuoteFeed { get; set; }
        public string ExchangeName { get; set; }
    }

    public class ServicesSettings
    {
        public string MarginTradingAssetService { get; set; }

    }
}
