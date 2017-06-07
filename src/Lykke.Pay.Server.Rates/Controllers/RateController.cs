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
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Pay.Service.Rates.Controllers
{
    [Route("api/[controller]")]
    public class RateController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly PayServiceRatesSettings _settings;
        private readonly IAssertPairHistoryRepository _assertPairHistoryRepository;
        private readonly HttpClient _client;
        private readonly RabbitMqSubscriber<PairRate> _subscriber;
        private DateTime _startCollectingInfo;

        public RateController(IMemoryCache cache, PayServiceRatesSettings settings, HttpClient client, RabbitMqSubscriber<PairRate> subscriber,
            IAssertPairHistoryRepository assertPairHistoryRepository)
        {
            _cache = cache;
            _settings = settings;
            _client = client;
            _assertPairHistoryRepository = assertPairHistoryRepository;

            _subscriber = subscriber;

        }


        // GET api/rate
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<AssertPairRate> cacheEntry;


            if (!_cache.TryGetValue(CacheKeys.Rates, out cacheEntry))
            {
                var respAssertPairList = await _client.GetAsync(new Uri(_settings.Services.MarginTradingAssetService));
                var sAssertPairList = await respAssertPairList.Content.ReadAsStringAsync();
                var pairsList = JsonConvert.DeserializeObject<List<MarginTradingAsset>>(sAssertPairList);
                cacheEntry = new List<AssertPairRate>();

                
                _subscriber
                    .SetMessageDeserializer(new MessageDeserializer())
                    .Subscribe(async rate =>
                    {
                       
                        var assertRate = cacheEntry.FirstOrDefault(ce => ce.AssetPair.Equals(rate.AssetPair));
                        if (assertRate == null)
                        {
                            assertRate = new AssertPairRate(rate);
                            cacheEntry.Add(assertRate);
                        }
                        else
                        {
                            assertRate.FillRate(rate);
                        }

                        assertRate.Accuracy = (from pl in pairsList
                                               where pl.Id.Equals(assertRate.AssetPair)
                                               select pl.Accuracy).FirstOrDefault();

                        await Task.FromResult(true);
                    });
                _startCollectingInfo = DateTime.UtcNow;
                _subscriber.Start();



               // var cntList = new List<AssertPairRate>(cacheEntry);
                List<AssertPairRate> listCompletedPairs;
                while ((listCompletedPairs = (from pl in pairsList
                        join ce in cacheEntry on pl.Id equals ce.AssetPair
                        where ce.IsReady()
                        select ce).ToList()).Count < _settings.AccessCrossCount)
                {
                    //var rates = string.Join("\n", pairsList.Select(r => r.Id));

                    if ((DateTime.UtcNow - _startCollectingInfo).TotalMilliseconds > _settings.SlowActivityTimeout)
                    {
                        
                        var cEntry =
                            new List<AssertPairRate>((from apr in (await _assertPairHistoryRepository.GetAllAsync())
                                                      orderby apr.StoredTime descending
                                                      select new AssertPairRate
                                                      {
                                                          AssetPair = apr.AssetPair,
                                                          Bid = (float)apr.Bid,
                                                          Ask = (float)apr.Ask,
                                                          Accuracy = apr.Accuracy
                                                      }).Take(_settings.AccessCrossCount));
                        if (cEntry.Count >= _settings.AccessCrossCount)
                        {
                            listCompletedPairs = new List<AssertPairRate>(cEntry);
                            break;
                        }
                        
                    }
                    //_assertPairHistoryRepository
                    await StoreAsserts(listCompletedPairs);
                    await Task.Delay(_settings.RabbitDelay);
                   
                }

                _subscriber.Stop();
                await StoreAsserts(listCompletedPairs);
                
                
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.CacheTimeout));

                cacheEntry = new List<AssertPairRate>(listCompletedPairs);
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
