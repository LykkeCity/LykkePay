﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Common.Log;
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

        public InvoicesController(PayApiSettings payApiSettings, HttpClient client, IInvoicesservice invoiceService, ILog log) : base(payApiSettings, client, log)
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
            entity.RowKey = entity.InvoiceId;
            entity.PartitionKey = MerchantId;
            entity.MerchantId = MerchantId;
            try
            {
                var resp = await _invoiceService.SaveInvoiceWithHttpMessagesAsync(entity);
                var result = resp.Body ?? false;
                if (!result)
                {
                    throw new DataException("Can't save invoice in Lykke Pay API");
                }
            }
            catch (Exception e)
            {
                await Log.WriteErrorAsync(nameof(InvoicesController), nameof(SaveInvoice), e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var context = new
            {
                entity.Transaction,
                entity.StartDate,
                entity.WalletAddress,
                entity.Status,
                entity.DueDate,
                entity.ClientUserId,
                entity.ClientId,
                entity.Currency,
                entity.Amount,
                entity.InvoiceNumber,
                entity.InvoiceId,
                MerchantId
            };
            await Log.WriteInfoAsync(nameof(InvoicesController), nameof(SaveInvoice), context.ToJson(), "Save new Invoce in system");

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
                var resp = await _invoiceService.GetInvoiceByIdWithHttpMessagesAsync(invoiceId);
                result = new InvoiceDetailResponse(resp.Body, _payApiSettings.LykkeInvoiceTemplate);
            }
            catch (Exception e)
            {
                await Log.WriteErrorAsync(nameof(InvoicesController), nameof(GetInvoice), e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Json(result);
        }

        [HttpPost("{invoiceId}/upload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFile(IFormFile file, string invoiceId)
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

            long size = file.Length;

            if (size > _payApiSettings.MaxUploadFileSize)
            {
                return BadRequest("Incorrect File Size");
            }
            
            var fileInfo = new IFileEntity
            {
                FileId = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                FileSize = (int)size,
                InvoiceId = invoiceId
            };

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileInfo.FileBodyBase64 = Convert.ToBase64String(ms.ToArray());
            }

            await _invoiceService.ApiInvoicesUploadFilePostWithHttpMessagesAsync(fileInfo);

            return Ok();
        }

    }
}
