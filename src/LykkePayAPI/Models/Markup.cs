using System;
using Lykke.Pay.Service.StoreRequest.Client.Models;

namespace LykkePay.API.Models
{
    public class Markup
    {
        public float Percent { get; set; }

        public int Pips { get; set; }

        public float FixedFee { get; set; }

        public static implicit operator IPayFee(Markup v)
        {
            return new IPayFee(v.Percent, v.Pips, v.FixedFee);
        }
    }
}
