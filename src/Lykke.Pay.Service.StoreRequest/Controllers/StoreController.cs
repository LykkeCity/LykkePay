using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Pay.Service.StoreRequest.Controllers
{
    [Route("api/Store")]
    public class StoreController : Controller
    {
      

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

       
    }
}
