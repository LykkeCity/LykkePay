using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lykke.Common.Entities.Dictionaries;
using Lykke.Pay.Service.Rates.Code;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Lykke.Common.Entities.Pay;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Pay.Service.Rates.Controllers
{
    [Route("api/[controller]")]
    public class RateController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly PayServiceRatesSettings _settings;
        private readonly HttpClient _client;
        private readonly RabbitMqSubscriber<PairRate> _subscriber;

        public RateController(IMemoryCache cache, PayServiceRatesSettings settings, HttpClient client,  RabbitMqSubscriber<PairRate> subscriber)
        {
            _cache = cache;
            _settings = settings;
            _client = client;

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
                _subscriber.Start();
               

                //using (var connection = _rabbitFactory.CreateConnection())
                //using (var channel = connection.CreateModel())
                //{
                //    var consumer = new EventingBasicConsumer(channel);
                //    consumer.Received += (model, ea) =>
                //    {
                //        var body = ea.Body;
                //        var message = Encoding.UTF8.GetString(body);
                //        var rate = JsonConvert.DeserializeObject<PairRate>(message);
                //        var assertRate = cacheEntry.FirstOrDefault(ce => ce.AssetPair.Equals(rate.AssetPair));
                //        if (assertRate == null)
                //        {
                //            assertRate = new AssertPairRate(rate);
                //            cacheEntry.Add(assertRate);
                //        }
                //        else
                //        {
                //            assertRate.FillRate(rate);
                //        }

                //        assertRate.Accuracy = (from pl in pairsList
                //                               where pl.Id.Equals(assertRate.AssetPair)
                //                               select pl.Accuracy).FirstOrDefault();

                //    };
                //    channel.BasicConsume(queue: _settings.RabbitMq.QuoteFeed, noAck: true, consumer: consumer);

                //    while ((from pl in pairsList
                //            join ce in cacheEntry on pl.Id equals ce.AssetPair
                //            where ce.IsReady()
                //            select ce).Count() < _settings.AccessCrossCount)
                //    {
                //        var loaded = pairsList.Where(pl => cacheEntry.Any(ce => ce.AssetPair.Equals(pl.Id))).ToList();
                //        var ready = cacheEntry.Where(ce => ce.IsReady()).ToList();
                //        await Task.Delay(_settings.RabbitDelay);
                //    }
                //}

                var cntList = new List<AssertPairRate>(cacheEntry);
                while ((from pl in pairsList
                        join ce in cntList on pl.Id equals ce.AssetPair
                        where ce.IsReady()
                        select ce).Count() < _settings.AccessCrossCount)
                {
                    var rates = string.Join("\n", pairsList.Select(r => r.Id));
                   // var items = (from pl in pairsList
                        //join ce in cntList on pl.Id equals ce.AssetPair
                        //where ce.IsReady()
                        //select ce).ToList();
                   // var realRates = string.Join("\n", items.Select(r => r.AssetPair));
                    //var loadedRates = string.Join("\n", cntList.Select(r => r.AssetPair));
                    //var loadedRatesReady = string.Join("\n", cntList.Where(r=>r.IsReady()).Select(r => r.AssetPair));
                    //var loaded = pairsList.Where(pl => cntList.Any(ce => ce.AssetPair.Equals(pl.Id))).ToList();
                    //var ready = cntList.Where(ce => ce.IsReady()).ToList();
                    await Task.Delay(_settings.RabbitDelay);
                    cntList = new List<AssertPairRate>(cacheEntry);
                }

                _subscriber.Stop();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.CacheTimeout));

                _cache.Set(CacheKeys.Rates, (from pl in pairsList
                                              join ce in cacheEntry on pl.Id equals ce.AssetPair
                                              where ce.IsReady()
                                              select ce).ToList(), cacheEntryOptions);
            }

            return Json(cacheEntry);
        }


    }
}
