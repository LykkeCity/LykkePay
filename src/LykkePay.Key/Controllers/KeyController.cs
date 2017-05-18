using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.Key.Controllers
{
    [Route("api/[controller]")]
    public class KeyController : Controller
    {

        [HttpGet("{id}")]
        public string Get(string id)
        {
            return "value";
        }
    }
}
