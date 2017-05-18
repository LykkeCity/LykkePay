using LykkePay.Business;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{
    [Route("api/[controller]")]
    public class VerifyController : Controller
    {
        private readonly SecurityHelper _securityHelper;

        public VerifyController(SecurityHelper securityHelper)
        {
            _securityHelper = securityHelper;
        }


        // POST api/verify
        [HttpPost]
        public IActionResult Post([FromBody]BaseRequest request)
        {
           return Json(new {result = _securityHelper.CheckRequest(request)});
        }

       

       
    }
}
