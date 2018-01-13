using System;
using System.Diagnostics;
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
            var guid = Guid.NewGuid().ToString();
            var merchantId = request.Headers["Lykke-Merchant-Id"].ToString();
            try
            {
                await _log.WriteInfoAsync("LykkePayApi", $"{request.Method} {request.Path}", $"merchantId: {merchantId}, requestid: {guid}", "receive request");

                var sw = new Stopwatch();
                sw.Start();
                await _next(context);
                sw.Stop();

                await _log.WriteInfoAsync("LykkePayApi", $"{request.Method} {request.Path}", $"merchantId: {merchantId}, requestid: {guid}", $"responce status code: {context.Response?.StatusCode}, {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                await _log.WriteWarningAsync("LykkePayApi", $"{request.Method} {request.Path}", $"merchantId: {merchantId}, requestid: {guid}", $"Exception on execute request", ex);
                throw;
            }
            
        }
    }
}