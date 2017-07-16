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


namespace Lykke.Pay.Service.Rates.Controllers
{
    [Route("api/[controller]")]
    public class RateController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly PayServiceRatesSettings _settings;
        private readonly IAssertPairHistoryRepository _assertPairHistoryRepository;
        private readonly HttpClient _client;


        public RateController(IMemoryCache cache, PayServiceRatesSettings settings, HttpClient client, IAssertPairHistoryRepository assertPairHistoryRepository)
        {
            _cache = cache;
            _settings = settings;
            _client = client;
            _assertPairHistoryRepository = assertPairHistoryRepository;

        }


        // GET api/rate
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<AssertPairRate> cacheEntry;


            if (!_cache.TryGetValue(CacheKeys.Rates, out cacheEntry))
            {
                var respAssertPairList = await _client.GetAsync(new Uri(_settings.Services.MarketProfileService));
                var sAssertPairList = await respAssertPairList.Content.ReadAsStringAsync();
                var pairsList = JsonConvert.DeserializeObject<List<dynamic>>(sAssertPairList);

                var dicAssertPairList = await _client.GetAsync(new Uri(_settings.Services.MarginTradingAssetService));
                var sDicAssertPairList = await dicAssertPairList.Content.ReadAsStringAsync();
                var dicPairsList = JsonConvert.DeserializeObject<List<MarginTradingAsset>>(sDicAssertPairList);



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
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.CacheTimeout));

                _cache.Set(CacheKeys.Rates, cacheEntry, cacheEntryOptions);
            }

            return Json(cacheEntry);
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
