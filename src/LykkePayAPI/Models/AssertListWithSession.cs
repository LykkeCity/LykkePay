using System.Collections.Generic;
using Lykke.Contracts.Pay;

namespace LykkePay.API.Models
{
    public class AssertListWithSession
    {
        public string SessionId { get; set; }
        public List<AssertPairRate> Asserts { get; set; }
    }
}