

namespace LykkePay.API.Models
{
    interface IStoreRequest
    {
        Lykke.Pay.Service.StoreRequest.Client.Models.MerchantPayRequest GetRequest();
    }
}
