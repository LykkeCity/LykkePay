using LykkePay.Business;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{
    [Route("api/[controller]")]
    public class VerifyController : Controller
    {
        private string merchantCertPublic = @"-----BEGIN CERTIFICATE-----
MIIDADCCAeygAwIBAgIQ4A+JAbs4op1HwTcls51w5zAJBgUrDgMCHQUAMBcxFTAT
BgNVBAMTDEx5a2tlUGF5bWVudDAeFw0xNzA1MTYwODQ5MTJaFw0zOTEyMzEyMzU5
NTlaMBsxGTAXBgNVBAMTEE1lcmNoYW50MVBheW1lbnQwggEiMA0GCSqGSIb3DQEB
AQUAA4IBDwAwggEKAoIBAQCigby1GHI44cuCx/JP8l8ysoygSKgTHHIj6wr1wWQE
8pT18ci/qvAhEQ7kfb1nXxh3jdPysH67QuFLS3CWaT4ZYRY1yOlUvYIIsnJlDY1N
7XGys1hpJ+E0sYiyh7uCKg071j3qbM6L6YalJ84NYFo5N+2iATO3N4ZHxgNUoET2
rw+WUTO33kDwRY4aJOwAGoUGw8XpcFPzAvWU4A1mxatBWEbM5xMgXukRkwiMjP7P
YssU3IOu2spFLlwe62hZ5t09bIDAxWUU4blJ7BR8IBz2IuTAMH4irN/fcSOL/+uH
isPYUI7TKhc9CKuw/Ax6yu7oTl9pGKdXTEYl7e0xUIX/AgMBAAGjTDBKMEgGA1Ud
AQRBMD+AEK0OmqbYffjsNWYOl+TETlyhGTAXMRUwEwYDVQQDEwxMeWtrZVBheW1l
bnSCENI5TgRKYm2PQuMsdi/zk2AwCQYFKw4DAh0FAAOCAQEAK6NiHD3/uGuvpQdk
w8iJ8ET6eoy0nTqrYQyFYn0f0uYO3qpZsFE4SsfBu4IC/r6yzR9wunZyY2sgWttx
CEYxeDBUKLlau/ZBLh115SNtUQP4Jzaumtp/4eeEJYzpoH/MH8h6XdxmO1CsEBOg
TCU/IogYY7sJ375SFhyI2s0na7o4xufW437JryjNo39S91VgZRa9LRrmofMJn2Jl
ctP8qwifsp/Q2uPjBPzmQ7cUjM211lFVjyt9r4DHJfv+UvVIspKgvNvM71KcQxDU
OBEswTDFCkpTVHvDIA5OpzI7J3a9XRAGPLv0ZViM4GU1TTcb37tHIY6qu2VpPgOu
nZzG+g==
-----END CERTIFICATE-----";

        // POST api/values
        [HttpPost]
        public void Post(BaseRequest request)
        {

        }

       

       
    }
}
