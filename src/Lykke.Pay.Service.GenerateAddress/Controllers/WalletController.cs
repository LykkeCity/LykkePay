using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.Contracts.Pay;
using Lykke.Core;
using Lykke.Pay.Service.GenerateAddress.Code;
using Lykke.Pay.Service.GenerateAddress.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lykke.Pay.Service.GenerateAddress.Controllers
{
    [Route("api/wallet")]
    public class WalletController : BaseController
    {
        private readonly IMerchantWalletRepository _merchantWalletRepository;
        private readonly IMerchantWalletHistoryRepository _merchantWalletHistoryRepository;
        public WalletController(PayServiceGenAddressSettings settings, IMerchantWalletRepository merchantWalletRepository, IMerchantWalletHistoryRepository merchantWalletHistoryRepository) : base(settings)
        {
            _merchantWalletRepository = merchantWalletRepository;
            _merchantWalletHistoryRepository = merchantWalletHistoryRepository;
        }


        private async Task<List<Tuple<string,AssertPrivKeyPair>>> GetWallets(string merchantId = null)
        {
            var wallets = string.IsNullOrEmpty(merchantId) ?  await _merchantWalletRepository.GetAllAddressAsync() :
                await _merchantWalletRepository.GetAllAddressOfMerchantAsync(merchantId);
            return (from w in wallets
                select new Tuple<string, AssertPrivKeyPair>(w.MerchantId, JsonConvert.DeserializeObject<AssertPrivKeyPair>(DecryptData(w.Data)))).ToList();
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<WalletInfo>> Get()
        {
            var wallets = await GetWallets();

            return from w in wallets
                   where !string.IsNullOrEmpty(w.Item2.Address)
                select new WalletInfo
                {
                    Amount = w.Item2.Amount,
                    Assert = w.Item2.AssertId,
                    WalletAddress = w.Item2.Address
                };

        }

        // GET api/values/5
        [HttpGet("{merchantId}")]
        public async Task<IEnumerable<WalletInfo>> Get(string merchantId)
        {
            if (string.IsNullOrEmpty(merchantId))
            {
                return null;
            }

            var wallets = await GetWallets(merchantId);
            return from w in wallets
                where !string.IsNullOrEmpty(w.Item2.Address)
                select new WalletInfo
                {
                    Amount = w.Item2.Amount,
                    Assert = w.Item2.AssertId,
                    WalletAddress = w.Item2.Address
                };
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody]List<WallerChangeRequest> changeRequest)
        {
            var wallets = await GetWallets();
            foreach (var cr in changeRequest)
            {
                if (string.IsNullOrEmpty(cr.WalletAddress))
                {
                    continue;
                }

                var w = wallets.FirstOrDefault(wa => cr.WalletAddress.Equals(wa.Item2.Address));
                if (w == null)
                {
                    continue;
                }

                w.Item2.Amount += cr.DeltaAmount;

                var encriptedData = EncryptData(JsonConvert.SerializeObject(w.Item2));
                await _merchantWalletRepository.SaveNewAddressAsync(new MerchantWalletEntity
                {
                    MerchantId = w.Item1,
                    WalletAddress = w.Item2.Address,
                    Data = encriptedData
                });
                var ipAddress = Request.HttpContext.GetRealIp();
                await _merchantWalletHistoryRepository.SaveNewChangeRequestAsync(cr.WalletAddress, cr.DeltaAmount, "NA", ipAddress);
            }

            
        }


        
    }
}
