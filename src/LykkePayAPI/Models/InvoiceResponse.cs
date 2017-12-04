using System.Collections.Generic;
using System.Runtime.Serialization;
using Lykke.Pay.Service.Invoces.Client.Models;

namespace LykkePay.API.Models
{
    [DataContract]
    public class InvoiceResponse
    {
        [DataMember(Name="date")]
        public string Date { get; set; }
        [DataMember(Name = "invoice_id")]
        public string InvoiceId { get; set; }
        [DataMember(Name = "invoice_number")]
        public string InvoiceNumber { get; set; }
        [DataMember(Name = "links")]
        public List<InvoiceLink> Links { get; set; } 
    



        public InvoiceResponse(IInvoiceEntity entity, string linkFormat)
        {
            Date = entity.DueDate;
            InvoiceId = entity.InvoiceId;
            InvoiceNumber = entity.InvoiceNumber;
            GenerateLinks(linkFormat);
        }

        protected void GenerateLinks(string linkFormat)
        {
            Links = new List<InvoiceLink>
            {
                new InvoiceLink
                {
                    Reltion = "generate",
                    Method = "GET",
                    Link = string.Format(linkFormat, InvoiceId)
                }
            };
        }
    }

    public static class InvoiceEntityExt
    {
        public static IInvoiceEntity Create(this InvoiceEntity entity)
        {
            return new IInvoiceEntity(entity.Amount, entity.InvoiceId, entity.InvoiceNumber, entity.Currency,
                entity.ClientId, entity.ClientName, entity.ClientUserId, entity.ClientEmail, entity.DueDate);
        }
    }

    
}