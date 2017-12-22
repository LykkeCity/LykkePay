using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Pay.Common
{
    public class OrderResponse
    {
        [DataContract]
        public class OrderSuccessReturn
        {
            public OrderSuccessReturn()
            {
                PaymentStatus = PaymentStatus.PAYMENT_CONFIRMED;
            }

            [JsonConverter(typeof(StringEnumConverter))]
            [DataMember(Name = "paymentStatus")]
            public PaymentStatus PaymentStatus { get; set; }

            [DataMember(Name = "paymentResponse")]
            public OrderSuccessResponse PaymentResponse { get; set; }

        }

        [DataContract]
        public class OrderInProgressReturn
        {
            public OrderInProgressReturn()
            {
                PaymentStatus = PaymentStatus.PAYMENT_INPROGRESS;
            }

            [JsonConverter(typeof(StringEnumConverter))]
            [DataMember(Name = "paymentStatus")]
            public PaymentStatus PaymentStatus { get; set; }

            [DataMember(Name = "paymentResponse")]
            public OrderInProgressResponse PaymentResponse { get; set; }

        }

        [DataContract]
        public class OrderErrorReturn
        {
            public OrderErrorReturn()
            {
                PaymentStatus = PaymentStatus.PAYMENT_ERROR;
            }

            [JsonConverter(typeof(StringEnumConverter))]
            [DataMember(Name = "paymentStatus")]
            public PaymentStatus PaymentStatus { get; set; }

            [DataMember(Name = "paymentResponse")]
            public TransferErrorResponse PaymentResponse { get; set; }

        }

        [DataContract]
        public class OrderSuccessResponse
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
        public class OrderInProgressResponse
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
        public class OrderErrorResponse
        {

            [DataMember(Name = "timestamp")]
            public long TimeStamp { get; set; }

            [DataMember(Name = "error")]
            [JsonConverter(typeof(StringEnumConverter))]
            public TransferError TransferError { get; set; }

        }

    }
}
