using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using LykkePay.Business.Interfaces;
using LykkePay.Business.Test.Models;

namespace LykkePay.Business.Test.Integrations
{
    public class WebSecurityHelper : ISecurityHelper
    {
        private const string ServiceUrl = "http://lykke-dev-pay.azurewebsites.net/api/verify";
        //private const string ServiceUrl = "http://localhost:4500/api/verify";
        private static HttpClient _client;
        private static readonly object Lock = new object();

        public WebSecurityHelper()
        {
            lock (Lock)
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                }
            }
        }

        public SecurityErrorType CheckRequest(BaseRequest request)
        {
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var result = _client.PostAsync(ServiceUrl, content).Result;
            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"The request returne error {(int)result.StatusCode}");
            }

            var val =
                Newtonsoft.Json.JsonConvert.DeserializeObject<IntegrationResult>(
                    result.Content.ReadAsStringAsync().Result);
            return val.Result;
        }
    }
}
