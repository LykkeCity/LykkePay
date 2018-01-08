using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.Common.Entities.Dictionaries;
using Lykke.Pay.Service.Rates.Code;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Lykke.Common.Entities.Pay;
using Lykke.Core;
using Lykke.Service.Assets.Client;
using Lykke.Service.MarketProfile.Client;


namespace Lykke.Pay.Service.Rates.Controllers
{
    [Route("api/[controller]")]
    public class RateController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly PayServiceRatesSettings _settings;
        private readonly IAssertPairHistoryRepository _assertPairHistoryRepository;
        private readonly ILykkeMarketProfile _lykkeMarketProfile;
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;


        public RateController(IMemoryCache cache, 
            PayServiceRatesSettings settings,
            IAssertPairHistoryRepository assertPairHistoryRepository,
            ILykkeMarketProfile lykkeMarketProfile,
            IAssetsServiceWithCache assetsServiceWithCache)
        {
            _cache = cache;
            _settings = settings;
            _assertPairHistoryRepository = assertPairHistoryRepository;
            _lykkeMarketProfile = lykkeMarketProfile;
            _assetsServiceWithCache = assetsServiceWithCache;
        }


        // GET api/rate
        [HttpGet]
        public async Task<IActionResult> Get(string sessionId, int? cacheTimeout)
        {
            List<AssertPairRate> cacheEntry;

            Guid sId;
            if (!Guid.TryParse(sessionId, out sId))
            {
                sId = Guid.NewGuid();
            }

            if (!_cache.TryGetValue(sId, out cacheEntry))
            {
                var pairsList = await _lykkeMarketProfile.ApiMarketProfileGetAsync();

                var dicPairsList = await _assetsServiceWithCache.GetAllAssetPairsAsync();

                cacheEntry = 
                    new List<AssertPairRate>(from rates in (from apr in pairsList

                                                            select new AssertPairRate
                                                            {
                                                                AssetPair = apr.AssetPair,
                                                                Bid = (float)apr.BidPrice,
                                                                Ask = (float)apr.AskPrice
                                                            })
                                             join dic in dicPairsList on rates.AssetPair equals dic.Id
                                             select new AssertPairRate
                                             {
                                                 AssetPair = rates.AssetPair,
                                                 Bid = rates.Bid,
                                                 Ask = rates.Ask,
                                                 Accuracy = dic.Accuracy
                                             }
                        );



                
                await StoreAsserts(cacheEntry);


                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes((cacheTimeout ?? 0) == 0 ? _settings.CacheTimeout : cacheTimeout.Value));

                _cache.Set(CacheKeys.Rates, cacheEntry, cacheEntryOptions);
            }

            return Json(new {SessionId = sId, Asserts = cacheEntry});
        }

        private async Task StoreAsserts(IEnumerable<AssertPairRate> assertPairRates)
        {
            foreach (var apr in assertPairRates)
            {
                await _assertPairHistoryRepository.SaveAssertPairHistoryAsync(AssertPairHistoryEntity.Create(apr));
            }
        }
    }
}
