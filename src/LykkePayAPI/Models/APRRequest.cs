using Lykke.Common.Entities.Pay;

namespace LykkePay.API.Models
{
    public class AprRequest
    {
        public AprRequest()
        {
            Markup = new AssertPairRateRequest();
        }

        public float Percent => Markup.Percent;

        public int Pips => Markup.Pips;

        public AssertPairRateRequest Markup { get; set; }
    }

    public class AprSafeRequest
    {
        public string Percent { get; set; }

        public string Pips { get; set; }

        public bool IsValid
        {
            get
            {
                float pr;
                int p;
                return (Percent == null || float.TryParse(Percent, out pr)) &&
                       (Pips == null || int.TryParse(Pips, out p));
            }
        }

        public bool AprRequest(out AprRequest request)
        {
            float pr =  0;
            int p = 0;
            request = null;
            var result = (Percent == null || float.TryParse(Percent, out pr)) &&
                   (Pips == null || int.TryParse(Pips, out p));
            if (result)
            {
                request = new AprRequest
                {
                    Markup =
                    {
                        Pips = p,
                        Percent = pr
                    }
                };
            };

            return result;
        }
    }

}
