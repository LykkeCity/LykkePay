using System;
using Lykke.Pay.Service.StoreRequest.Client.Models;

namespace LykkePay.API.Models
{
    public class Markup
    {
        public double Percent { get; set; }

        public int Pips { get; set; }

        public double FixedFee { get; set; }

        
    }
}
