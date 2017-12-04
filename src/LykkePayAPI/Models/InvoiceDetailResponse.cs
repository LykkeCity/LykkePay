using System;
using System.Runtime.Serialization;
using Lykke.Pay.Service.Invoces.Client.Models;

namespace LykkePay.API.Models
{
    [DataContract]
    public class InvoiceDetailResponse : InvoiceResponse
    {
        [DataMember(Name= "client")]
        public string Client  { get;set;}
        [DataMember(Name = "client_email")]
        public string ClientEmail { get;set;}
        [DataMember(Name = "amount")]
        public double Amount { get;set;}
        [DataMember(Name = "currency")]
        public string Currency { get;set;}
        [DataMember(Name = "label")]
        public string Label { get;set;}
        
        public InvoiceDetailResponse(IInvoiceEntity entity, string linkFormat) : base(entity, linkFormat)
        {
            Client = entity.ClientName;
            ClientEmail = entity.ClientEmail;
            Amount = entity.Amount;
            Currency = entity.Currency;
            Label = entity.Label;
        }
    }
}