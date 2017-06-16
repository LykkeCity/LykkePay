using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{

    [Route("api/Order")]
    public class OrderController : BaseController
    {
        public OrderController(PayApiSettings payApiSettings, HttpClient client) : base(payApiSettings, client)
        {
        }
    }
}