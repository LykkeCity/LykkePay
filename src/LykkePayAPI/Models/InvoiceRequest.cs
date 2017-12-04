using System;
using System.Runtime.Serialization;
using Lykke.Pay.Service.Invoces.Client.Models;

namespace LykkePay.API.Models
{
    [DataContract]
    public class InvoiceRequest
    {
        [DataMember(Name = "invoice_number")]
        public string InvoiceNumber { get; set; }

        [DataMember(Name = "client")]
        public string Client { get; set; }

        [DataMember(Name = "client_email")]
        public string ClientEmail { get; set; }

        [DataMember(Name = "amount")]
        public double Amount { get; set; }

        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }

        [DataMember(Name = "due_date")]
        //yyyy-mm-dd z
        public string DueDate { get; set; }

        public InvoiceEntity CreateEntity()
        {
            if (string.IsNullOrEmpty(Client) || string.IsNullOrEmpty(ClientEmail) || Amount <= 0 ||
                string.IsNullOrEmpty(Currency))
            {
                return null;
            }

            return

            new InvoiceEntity
            {
                Amount = Amount,
                ClientEmail = ClientEmail,
                ClientName = Client,
                Currency = Currency,
                InvoiceNumber = InvoiceNumber,
                InvoiceId = Guid.NewGuid().ToString(),
                DueDate = DueDate,
                Label = Label
            };
        }
    }
}