namespace Lykke.Pay.Service.StoreRequest.Code
{
    public class Settings
    {
        public PayServiceStoreRequestSettings PayServiceStoreRequest { get; set; }

    }
    public class PayServiceStoreRequestSettings
    {
        public DbSettings Db { get; set; }
    }



    public class DbSettings
    {
        public string RequestStoreConnString { get; set; }

    }

}
