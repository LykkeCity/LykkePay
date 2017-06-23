using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Core;

namespace Lykke.Pay.Service.StoreRequest.Models
{
    public class PayRequest : IMerchantPayRequest
    {
        public string MerchantId { get; set; }
        public string RequestId { get; set; }
        public string TransactionId { get; set; }
        public PayFee Markup { get; set; }
        public MerchantPayRequestStatus MerchantPayRequestStatus { get; set; }
        public MerchantPayRequestType MerchantPayRequestType { get; set; }
        public MerchantPayRequestNotification MerchantPayRequestNotification { get; set; }
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string AssetPair { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string SuccessUrl { get; set; }
        public string ErrorUrl { get; set; }
        public string ProgressUrl { get; set; }
        public string OrderId { get; set; }
    }
}
