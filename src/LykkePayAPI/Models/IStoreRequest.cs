using Lykke.AzureRepositories;

namespace LykkePay.API.Models
{
    interface IStoreRequest
    {
        MerchantPayRequest GetRequest();
    }
}
