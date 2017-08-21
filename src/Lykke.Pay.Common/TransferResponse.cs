using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Pay.Common
{
    [DataContract]
    public class TransferSuccessReturn
    {
        public TransferSuccessReturn()
        {
            TransferStatus = TransferStatus.TRANSFER_CONFIRMED;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "transferStatus")]
        public TransferStatus TransferStatus { get; set; }

        [DataMember(Name = "transferResponse")]
        public TransferSuccessResponse TransferResponse { get; set; }

    }

    [DataContract]
    public class TransferInProgressReturn
    {
        public TransferInProgressReturn()
        {
            TransferStatus = TransferStatus.TRANSFER_INPROGRESS;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "transferStatus")]
        public TransferStatus TransferStatus { get; set; }

        [DataMember(Name = "transferResponse")]
        public TransferInProgressResponse TransferResponse { get; set; }

    }

    [DataContract]
    public class TransferErrorReturn
    {
        public TransferErrorReturn()
        {
            TransferStatus = TransferStatus.TRANSFER_ERROR;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "transferStatus")]
        public TransferStatus TransferStatus { get; set; }

        [DataMember(Name = "transferResponse")]
        public TransferErrorResponse TransferResponse { get; set; }

    }

    [DataContract]
    public class TransferSuccessResponse
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
    public class TransferInProgressResponse
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
    public class TransferErrorResponse
    {

        [DataMember(Name = "timestamp")]
        public long TimeStamp { get; set; }

        [DataMember(Name = "error")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransferError TransferError { get; set; }

    }


}
