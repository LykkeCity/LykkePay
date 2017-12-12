namespace Lykke.Pay.Job.ProcessRequests
{
    public class Settings
    {
        public LykkePayJobProcessRequestsSettings LykkePayJobProcessRequests { get; set; }

    }
    public class LykkePayJobProcessRequestsSettings
    {
       // public RpcSettings Rpc { get; set; }
       // public int NumberOfConfirm { get; set; }
        public DbSettings Db { get; set; }

        public ServicessSettings Services { get; set; }
        //  public string DataEncriptionPassword { get; set; }
    }

    public class ServicessSettings
    {
        public string LykkePayServiceStoreRequestMicroService { get; set; }
        public string BitcoinApiService { get; set; }
    }

    public class DbSettings
    {
        public string MerchantWalletConnectionString { get; set; }
    }
    //public class RpcSettings
    //{
    //    public string UserName { get; set; }

    //    public string Password { get; set; }

    //    public string Url { get; set; }
        
    //}
}
