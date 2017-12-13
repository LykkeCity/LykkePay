using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Core;
using Lykke.Pay.Service.Invoces.Client;
using Lykke.Pay.Service.Invoces.Client.Models;
using LykkePay.API.Code;
using LykkePay.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LykkePay.API.Controllers
{
    [Route("api/v1/Invoices")]
    public class InvoicesController : BaseController
    {
        private readonly IInvoicesservice _invoiceService;
        private readonly PayApiSettings _payApiSettings;

        public InvoicesController(PayApiSettings payApiSettings, HttpClient client, IInvoicesservice invoiceService ) : base(payApiSettings, client)
        {
            _invoiceService = invoiceService;
            _payApiSettings = payApiSettings;
        }

        [HttpPost]
        public async Task<IActionResult> SaveInvoice([FromBody]InvoiceRequest request)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            var entity = request?.CreateEntity();
            if (entity == null)
            {
                return BadRequest();
            }

            try
            {
                var resp = await _invoiceService.ApiInvoicesPostWithHttpMessagesAsync(entity);
                var result = resp.Body ?? false;
                if (!result)
                {
                    throw new DataException("Can't save invoice in Lykke Pay API");
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Json(new InvoiceResponse(entity.Create(), _payApiSettings.LykkeInvoiceTemplate));

        }

        [HttpGet(@"{invoiceId}")]
        public async Task<IActionResult> GetInvoice(string invoiceId)
        {
            var isValid = await ValidateRequest();
            if ((isValid as OkResult)?.StatusCode != Ok().StatusCode)
            {
                return isValid;
            }

            if (string.IsNullOrEmpty(invoiceId))
            {
                return BadRequest();
            }

            InvoiceDetailResponse result;
            try
            {
                var resp = await _invoiceService.ApiInvoicesByInvoiceIdGetWithHttpMessagesAsync(invoiceId);
                result = new InvoiceDetailResponse(resp.Body, _payApiSettings.LykkeInvoiceTemplate);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Json(result);
        }

    }
}
