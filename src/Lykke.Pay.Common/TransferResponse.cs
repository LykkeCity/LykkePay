using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Pay.Common
{
    public enum TransferError
    {
        NotError,
        TransferNotConfirmed,
        InvalidAmount,
        InvalidAddress,
        InternalError
    }
    public enum TransferStatus
    {
        TransferConfirmed,
        TransferInProgress,
        TransferError
    }
    public enum Settlement
    {
        NotApplicable,
        TransactionDetected
    }
    public class TransferReturn
    {
        public TransferReturn()
        {
            TransferResponse = new TransferResponse
            {
                Settlement = Settlement.NotApplicable,
                TransferError = TransferError.NotError
            };
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TransferStatus TransferStatus { get; set; }

        public TransferResponse TransferResponse { get; set; }

}

    
    public class TransferResponse
    {
        public string Currency { get; set; }

        public long TimeStamp { get; set; }

        public int NumberOfConfirmation { get; set; }

        public string Url { get; set; }

        public string TransactionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Settlement Settlement { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TransferError TransferError { get; set; }


    }

   
}
