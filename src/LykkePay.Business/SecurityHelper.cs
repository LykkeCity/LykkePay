using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using LykkePay.Business.Interfaces;

namespace LykkePay.Business
{
    public class SecurityHelper : ISecurityHelper
    {
        private readonly string _merchantCertPublic = @"-----BEGIN CERTIFICATE-----
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

        // private readonly IConfigurationRoot _configuration;

        //public SecurityHelper(IConfigurationRoot configuration)
        //{
        //    _configuration = configuration;
        //}

      
        public SecurityErrorType CheckRequest(BaseRequest request)
        {
            if (request.MerchantId != "1")
            {
                return SecurityErrorType.MerchantUnknown;
            }

            if (string.IsNullOrEmpty(request.Sign))
            {
                return SecurityErrorType.SignEmpty;
            }

            if ((DateTime.UtcNow - request.RequestDate.ToUniversalTime()).TotalMinutes > 5)
            {
                return SecurityErrorType.OutOfDate;
            }

            string stringToSign = GetStringToSign(request);
            var cert = GetCertificate(_merchantCertPublic);

            var rsa = cert.GetRSAPublicKey();
            bool isCorrect = false;
            try
            {
                isCorrect = rsa.VerifyData(Encoding.UTF8.GetBytes(stringToSign), Convert.FromBase64String(request.Sign), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            }
            catch { }
            
          
            return isCorrect ? SecurityErrorType.Ok : SecurityErrorType.SignIncorrect;
        }

        private X509Certificate2 GetCertificate(string cert)
        {
            cert = cert.Replace("-----BEGIN CERTIFICATE-----", string.Empty);
            cert = cert.Replace("-----END CERTIFICATE-----", string.Empty);
            cert =   new Regex("(?is)\\s+").Replace(cert, string.Empty);
            return new X509Certificate2(Convert.FromBase64String(cert));
        }

        private string GetStringToSign(BaseRequest request)
        {
            return string.Join(string.Empty, from p in request.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                let val =
                (p.PropertyType == typeof(DateTime)
                    ? ((DateTime) p.GetValue(request)).ToUniversalTime().ToString("yyyy-MM-dd hh:mm:ss")
                    : p.GetValue(request).ToString())
                where !p.Name.Equals("Sign", StringComparison.CurrentCultureIgnoreCase) && p.CanRead && p.CanWrite
                orderby p.Name
                select val);
        }
    }
}
