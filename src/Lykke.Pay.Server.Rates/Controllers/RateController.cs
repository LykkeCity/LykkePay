using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Pay.Server.Rates.Code;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Pay.Server.Rates.Controllers
{
    [Route("api/[controller]")]
    public class RateController : Controller
    {
        private IMemoryCache _cache;


        public RateController(IMemoryCache cache)
        {
            _cache = cache;
        }


        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            DateTime cacheEntry;

            // Look for cache key.
            if (!_cache.TryGetValue(CacheKeys.Entry, out cacheEntry))
            {
                // Key not in cache, so get data.
                cacheEntry = DateTime.Now;

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(5));

                // Save data in cache.
                _cache.Set(CacheKeys.Entry, cacheEntry, cacheEntryOptions);
            }

            return Content(cacheEntry.ToString("u"));
        }

        // GET api/values/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
