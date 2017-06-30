using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Microsoft.AspNetCore.Mvc;
using Lykke.Common.Entities.Pay;
using Lykke.Common.Entities.Security;
using Lykke.Core;
using Lykke.Pay.Service.GenerateAddress.Code;
using Lykke.Signing.Client;
using Newtonsoft.Json;

namespace Lykke.Pay.Service.GenerateAddress.Controllers
{

    [Route("api/generate")]
    public class GenerateController : BaseController
    {
        private readonly IMerchantWalletRepository _merchantWalletRepository;
        private readonly ILykkeSigningAPI _lykkeSigningApi;

        public GenerateController(PayServiceGenAddressSettings settings, IMerchantWalletRepository merchantWalletRepository, ILykkeSigningAPI lykkeSigningApi) : base(settings)
        {
            _merchantWalletRepository = merchantWalletRepository;
            _lykkeSigningApi = lykkeSigningApi;

        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]GenerateAddressRequest request)
        {
            if (string.IsNullOrEmpty(request?.MerchantId) || string.IsNullOrEmpty(request.AssertId))
            {
                return Json(new {Address = string.Empty});
            }


            var keyInfo = await _lykkeSigningApi.ApiBitcoinKeyGetWithHttpMessagesAsync();
            var publicKey = keyInfo.Body;

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

            return Json(new { publicKey.Address });

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
