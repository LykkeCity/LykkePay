using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Pay.Common
{
    public class TransferSuccessReturn
    {
        public TransferSuccessReturn()
        {
            TransferStatus = TransferStatus.TRANSFER_CONFIRMED;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TransferStatus TransferStatus { get; set; }

        public TransferSuccessResponse TransferResponse { get; set; }

    }

    public class TransferInProgressReturn
    {
        public TransferInProgressReturn()
        {
            TransferStatus = TransferStatus.TRANSFER_INPROGRESS;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TransferStatus TransferStatus { get; set; }

        public TransferInProgressResponse TransferResponse { get; set; }

    }

    public class TransferErrorReturn
    {
        public TransferErrorReturn()
        {
            TransferStatus = TransferStatus.TRANSFER_ERROR;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TransferStatus TransferStatus { get; set; }

        public TransferErrorResponse TransferResponse { get; set; }

    }


    public class TransferSuccessResponse
    {
        public string Currency { get; set; }

        public long TimeStamp { get; set; }

        public int NumberOfConfirmation { get; set; }

        public string Url { get; set; }

        public string TransactionId { get; set; }



    }

    public class TransferInProgressResponse
    {
        public string Currency { get; set; }

        public long TimeStamp { get; set; }

        public string TransactionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Settlement Settlement { get; set; }


    }

    public class TransferErrorResponse
    {
      
        public long TimeStamp { get; set; }

       
        [JsonConverter(typeof(StringEnumConverter))]
        public TransferError TransferError { get; set; }

    }


}
