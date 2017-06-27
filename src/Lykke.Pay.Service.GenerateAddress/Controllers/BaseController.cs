using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lykke.Pay.Service.GenerateAddress.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Lykke.Pay.Service.GenerateAddress.Controllers
{
    public class BaseController : Controller
    {
        protected PayServiceGenAddressSettings Settings { get; set; }

        protected BaseController(PayServiceGenAddressSettings settings)
        {
            Settings = settings;
        }
        protected string EncryptData(string data)
        {

            byte[] result;
            using (var aes = Aes.Create())
            using (var md5 = MD5.Create())
            using (var sha256 = SHA256.Create())
            {
                aes.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(Settings.DataEncriptionPassword));
                aes.IV = md5.ComputeHash(Encoding.UTF8.GetBytes(Settings.DataEncriptionPassword));

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    result = resultStream.ToArray();
                }
            }

            return Convert.ToBase64String(result);
        }


        protected string DecryptData(string data)
        {

            byte[] result;
            using (var aes = Aes.Create())
            using (var md5 = MD5.Create())
            using (var sha256 = SHA256.Create())
            {
                aes.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(Settings.DataEncriptionPassword));
                aes.IV = md5.ComputeHash(Encoding.UTF8.GetBytes(Settings.DataEncriptionPassword));

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
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
