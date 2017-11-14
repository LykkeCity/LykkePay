using System;
using System.Runtime.Serialization;
using Lykke.Common.Entities.Pay;

namespace LykkePay.API.Models
{
    [DataContract]
    public class AssertPairRateWithSession : AssertPairRate
    {
        [DataMember(Name = "Lykke-Merchant-Session-Id")]
        public string SessionId { get; set; }

        public AssertPairRateWithSession(AssertPairRate rate, string newSessionId) : base(rate)
        {
            SessionId = newSessionId;
        }
    }
}