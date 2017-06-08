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
using Newtonsoft.Json;

namespace Lykke.Pay.Service.GenerateAddress.Controllers
{

    [Route("api/generate")]
    public class GenerateController : Controller
    {
        private readonly PayServiceGenAddressSettings _settings;
        private readonly IMerchantWalletRepository _merchantWalletRepository;

        public GenerateController(PayServiceGenAddressSettings settings, IMerchantWalletRepository merchantWalletRepository)
        {
            _settings = settings;
            _merchantWalletRepository = merchantWalletRepository;
        }

        // POST api/values
        [HttpPost]
        public async Task<string> Post([FromBody]GenerateAddressRequest request)
        {
            if (string.IsNullOrEmpty(request?.MerchantId) || string.IsNullOrEmpty(request.AssertId))
            {
                return string.Empty;
            }

            using (var rsa = RSA.Create())
            {
                rsa.KeySize = _settings.GenerateKeySize;
                var pbl = rsa.ExportParameters(true);

                var dateToStore = new AssertPrivKeyPair
                {
                    AssertId = request.AssertId,
                    PrivateKey = pbl.GetPrivateKey()
                };

                var encriptedData = SavePrimaryKey(JsonConvert.SerializeObject(dateToStore));
                await _merchantWalletRepository.SaveNewAddress(new MerchantWalletEntity
                {
                    MerchantId = request.MerchantId,
                    Data = encriptedData
                });
                return pbl.GetPublicKey();
            }

        }

        private string SavePrimaryKey(string getPrivateKey)
        {

            byte[] result;
            using (var aes = Aes.Create())
            using (var md5 = MD5.Create())
            using (var sha256 = SHA256.Create())
            {
                aes.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(_settings.DataEncriptionPassword));
                aes.IV = md5.ComputeHash(Encoding.UTF8.GetBytes(_settings.DataEncriptionPassword));

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(Encoding.UTF8.GetBytes(getPrivateKey)))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    result = resultStream.ToArray();
                }
            }

            return Convert.ToBase64String(result);
        }
    }
}
