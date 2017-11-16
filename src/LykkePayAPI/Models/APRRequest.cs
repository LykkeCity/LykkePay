using Lykke.Common.Entities.Pay;

namespace LykkePay.API.Models
{
    public class AprRequest
    {
        public float Percent => Markup.Percent;

        public int Pips => Markup.Pips;

        public AssertPairRateRequest Markup { get; set; }
    }

    
    
}
