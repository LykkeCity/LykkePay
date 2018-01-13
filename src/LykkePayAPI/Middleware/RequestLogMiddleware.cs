using System;
using System.Threading.Tasks;
using Common.Log;
using Microsoft.AspNetCore.Http;

namespace LykkePay.API.Middleware
{
    public class RequestLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILog _log;

        public RequestLogMiddleware(RequestDelegate next, ILog log)
        {
            _next = next;
            _log = log;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var merchantId = request.Headers["Lykke-Merchant-Id"].ToString();
            try
            {
                await _log.WriteInfoAsync("LykkePayApi", $"{request.Method} {request.Path}", $"merchantId: {merchantId}", "receive request");

                await _next(context);

                await _log.WriteInfoAsync("LykkePayApi", $"{request.Method} {request.Path}", $"merchantId: {merchantId}", $"responce status code: {context.Response?.StatusCode}");
            }
            catch (Exception ex)
            {
                await _log.WriteWarningAsync("LykkePayApi", $"{request.Method} {request.Path}", $"merchantId: {merchantId}", $"Exception on execute request", ex);
                throw;
            }
            
        }
    }
}