

namespace LykkePay.API.Models
{
    interface IStoreRequest
    {
        Lykke.Pay.Service.StoreRequest.Client.Models.IMerchantPayRequest GetRequest();
    }

}
