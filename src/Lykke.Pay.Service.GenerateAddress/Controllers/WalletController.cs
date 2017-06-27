using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.Common.Entities.Pay;
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


        private async Task<List<Tuple<string,AssertPrivKeyPair>>> GetWallets()
        {
            var wallets = await _merchantWalletRepository.GetAllAddressAsync();
            return (from w in wallets
                select new Tuple<string, AssertPrivKeyPair>(w.MerchantId, JsonConvert.DeserializeObject<AssertPrivKeyPair>(DecryptData(w.Data)))).ToList();
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<WalletInfo>> Get()
        {
            var wallets = await GetWallets();
            return from w in wallets
                select new WalletInfo
                {
                    Amount = w.Item2.Amount,
                    Assert = w.Item2.AssertId,
                    WalletAddress = w.Item2.Address
                };

        }

        // GET api/values/5
        [HttpGet("{walletAddress}")]
        public async Task<WalletInfo> Get(string walletAddress)
        {
            var wallets = await GetWallets();
            return (from w in wallets
                    where w.Item2.Address.Equals(walletAddress)
                select new WalletInfo
                {
                    Amount = w.Item2.Amount,
                    Assert = w.Item2.AssertId,
                    WalletAddress = w.Item2.Address
                }).FirstOrDefault();
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody]List<WallerChangeRequest> changeRequest)
        {
            var wallets = await GetWallets();
            foreach (var cr in changeRequest)
            {
                var w = wallets.FirstOrDefault(wa => wa.Item2.Address.Equals(cr.WalletAddress));
                if (w == null)
                {
                    continue;
                }

                w.Item2.Amount += cr.DeltaAmount;

                var encriptedData = EncryptData(JsonConvert.SerializeObject(w));
                await _merchantWalletRepository.SaveNewAddressAsync(new MerchantWalletEntity
                {
                    MerchantId = w.Item1,
                    Data = encriptedData
                });
                var ipAddress = Request.HttpContext.GetRealIp();
                await _merchantWalletHistoryRepository.SaveNewChangeRequestAsync(cr.WalletAddress, cr.DeltaAmount, "NA", ipAddress);
            }

            
        }


        
    }
}
