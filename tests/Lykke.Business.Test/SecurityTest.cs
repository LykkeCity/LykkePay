using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LykkePay.Business.Test.Models;
using LykkePay.Business;
using Xunit;

namespace LykkePay.Business.Test
{
    public class SecurityTest 
    {
        private readonly SecurityHelper _securityHelper;
        public SecurityTest()
        {
            _securityHelper = new SecurityHelper();
        }

        [Fact]
        public void CheckUncorrectMerchant()
        {
            var result = _securityHelper.CheckRequest(new BaseRequest
            {
                MerchantId = "100",
                RequestDate = DateTime.Now,
                Sign = String.Empty
            });

            Assert.Equal(SecurityErrorType.MerchantUnknown, result);
        }

        [Fact]
        public void CheckSignEmpty()
        {
            var result = _securityHelper.CheckRequest(new BaseRequest
            {
                MerchantId = "1",
                RequestDate = DateTime.Now,
                Sign = String.Empty
            });

            Assert.Equal(SecurityErrorType.SignEmpty, result);
        }

        [Fact]
        public void CheckSignIncorrect()
        {
            var result = _securityHelper.CheckRequest(new BaseRequest
            {
                MerchantId = "1",
                RequestDate = DateTime.Now,
                Sign = "test"
            });

            Assert.Equal(SecurityErrorType.SignIncorrect, result);
        }

        [Fact]
        public void CheckSignOutOfDate()
        {
            var result = _securityHelper.CheckRequest(new BaseRequest
            {
                MerchantId = "1",
                RequestDate = DateTime.Now.AddDays(-5),
                Sign = "test"
            });

            Assert.Equal(SecurityErrorType.OutOfDate, result);
        }

        [Fact]
        public void CheckSignCorrect()
        {

            var date = DateTime.Now;
            string strToSign = $"1{date:yyyy-MM-dd hh:mm:ss}";
            X509Certificate2 certificate;
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                certificate = store.Certificates.Cast<X509Certificate2>().First(c => c.SubjectName.Name.IndexOf("CN=Merchant1", StringComparison.Ordinal) >= 0) ;
            }
            
            var csp = certificate.GetRSAPrivateKey();
            var sign = Convert.ToBase64String(csp.SignData(Encoding.UTF8.GetBytes(strToSign), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1));

            var result = _securityHelper.CheckRequest(new BaseRequest
            {
                MerchantId = "1",
                RequestDate = date,
                Sign = sign
            });

            Assert.Equal(SecurityErrorType.Ok, result);
        }

        [Fact]
        public void CheckSignCorrectCustomRequest()
        {

            var date = DateTime.Now;
            string strToSign = $"1{date:yyyy-MM-dd hh:mm:ss}ttt101asd555";
            X509Certificate2 certificate;
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                certificate = store.Certificates.Cast<X509Certificate2>().First(c => c.SubjectName.Name.IndexOf("CN=Merchant1", StringComparison.Ordinal) >= 0);
            }

            var csp = certificate.GetRSAPrivateKey();
            var sign = Convert.ToBase64String(csp.SignData(Encoding.UTF8.GetBytes(strToSign), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1));

            var result = _securityHelper.CheckRequest(new TestRequest
            {
                MerchantId = "1",
                RequestDate = date,
                Sign = sign,
                Test1 = "ttt",
                Test2 = 101,
                Test4 = 555,
                Test3 = "asd"
            });

            Assert.Equal(SecurityErrorType.Ok, result);
        }
    }
}
