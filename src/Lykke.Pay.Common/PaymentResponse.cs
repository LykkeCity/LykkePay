using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Pay.Common
{
    [DataContract]
    public class PaymentSuccessReturn
    {
        public PaymentSuccessReturn()
        {
            PaymentStatus = PaymentStatus.PAYMENT_CONFIRMED;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "PaymentStatus")]
        public PaymentStatus PaymentStatus { get; set; }

        [DataMember(Name = "PaymentResponse")]
        public PaymentSuccessResponse PaymentResponse { get; set; }

    }

    [DataContract]
    public class PaymentInProgressReturn
    {
        public PaymentInProgressReturn()
        {
            PaymentStatus = PaymentStatus.PAYMENT_INPROGRESS;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "PaymentStatus")]
        public PaymentStatus PaymentStatus { get; set; }

        [DataMember(Name = "PaymentResponse")]
        public PaymentInProgressResponse PaymentResponse { get; set; }

    }

    [DataContract]
    public class PaymentErrorReturn
    {
        public PaymentErrorReturn()
        {
            PaymentStatus = PaymentStatus.PAYMENT_ERROR;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "PaymentStatus")]
        public PaymentStatus PaymentStatus { get; set; }

        [DataMember(Name = "PaymentResponse")]
        public PaymentErrorResponse PaymentResponse { get; set; }

    }

    [DataContract]
    public class PaymentSuccessResponse
    {
        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "timestamp")]
        public long TimeStamp { get; set; }

        [DataMember(Name = "numberOfConfirmations")]
        public int NumberOfConfirmation { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "transactionId")]
        public string TransactionId { get; set; }



    }

    [DataContract]
    public class PaymentInProgressResponse
    {
        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "timestamp")]
        public long TimeStamp { get; set; }

        [DataMember(Name = "transactionId")]
        public string TransactionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "settlement")]
        public Settlement Settlement { get; set; }


    }

    [DataContract]
    public class PaymentErrorResponse
    {

        [DataMember(Name = "timestamp")]
        public long TimeStamp { get; set; }

        [DataMember(Name = "error")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransferError PaymentError { get; set; }

    }
}
