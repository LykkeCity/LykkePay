namespace Lykke.Pay.Job.BitcointBlocksHandle
{
    public class Settings
    {
        public LykkePayJobBitcointHandleSettings LykkePayJobBitcointHandle { get; set; }

    }
    public class LykkePayJobBitcointHandleSettings
    {
        public RpcSettings Rpc { get; set; }
        public int NumberOfConfirm { get; set; }
        public DbSettings Db { get; set; }
        public string DataEncriptionPassword { get; set; }
    }

    public class DbSettings
    {
        public string MerchantWalletConnectionString { get; set; }
    }
    public class RpcSettings
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Url { get; set; }
        
    }
}
