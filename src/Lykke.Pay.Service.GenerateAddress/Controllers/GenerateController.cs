using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Bitcoint.Api.Client.Models;
using Lykke.AzureRepositories;
using Microsoft.AspNetCore.Mvc;
using Lykke.Common.Entities.Pay;
using Lykke.Core;
using Lykke.Pay.Service.GenerateAddress.Code;
using Lykke.Pay.Service.GenerateAddress.Models;
using Newtonsoft.Json;

namespace Lykke.Pay.Service.GenerateAddress.Controllers
{

    [Route("api/generate")]
    public class GenerateController : BaseController
    {
        private readonly IMerchantWalletRepository _merchantWalletRepository;
        private readonly IBitcoinApi _bitcoinApi;

        public GenerateController(PayServiceGenAddressSettings settings, IMerchantWalletRepository merchantWalletRepository, IBitcoinApi bitcoinApi) : base(settings)
        {
            _merchantWalletRepository = merchantWalletRepository;
            _bitcoinApi = bitcoinApi;

        }

        // POST api/values
        [HttpPost]
        public async Task<GenerateAddressModel> Post([FromBody]GenerateAddressRequest request)
        {
            if (string.IsNullOrEmpty(request?.MerchantId) || string.IsNullOrEmpty(request.AssertId))
            {
                return new GenerateAddressModel { Address = string.Empty};
            }


            var keyInfo = await _bitcoinApi.ApiWalletLykkepayGeneratePostWithHttpMessagesAsync();
            var publicKey = (GenerateWalletResponse)keyInfo.Body;

            var dateToStore = new AssertPrivKeyPair
            {
                AssertId = request.AssertId,
                PublicKey = publicKey.PubKey,
                Address = publicKey.Address
            };

            var encriptedData = EncryptData(JsonConvert.SerializeObject(dateToStore));
            await _merchantWalletRepository.SaveNewAddressAsync(new MerchantWalletEntity
            {
                MerchantId = request.MerchantId,
                WalletAddress = publicKey.Address,
                Data = encriptedData
            });

            return new GenerateAddressModel { Address = publicKey.Address };

            //using (var rsa = RSA.Create())
            //{
            //    rsa.KeySize = _settings.GenerateKeySize;
            //    var pbl = rsa.ExportParameters(true);

            //    var dateToStore = new AssertPrivKeyPair
            //    {
            //        AssertId = request.AssertId,
            //        PrivateKey = pbl.GetPrivateKey()
            //    };

            //    var encriptedData = SavePrimaryKey(JsonConvert.SerializeObject(dateToStore));
            //    await _merchantWalletRepository.SaveNewAddress(new MerchantWalletEntity
            //    {
            //        MerchantId = request.MerchantId,
            //        Data = encriptedData
            //    });
            //    return pbl.GetPublicKey();
            //}

        }


    }
}
