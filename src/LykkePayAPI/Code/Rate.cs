using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LykkePay.API.Code
{
    public class Rate
    {
        private bool _filledBit = false;
        private bool _filledAsk = false;



        public Rate(dynamic source)
        {
            AssetPair = source.AssetPair;
            FillRate(source);
        }

        public void FillRate(dynamic source)
        {
            if ((bool)source.IsBuy)
            {
                Bid = source.Price;
                _filledBit = true;
            }
            else
            {
                Ask = source.Price;
                _filledAsk = true;
            }
        }

        public string AssetPair { get; private set; }
        public double Bid { get; private set; }
        public double Ask { get; private set; }

        public bool IsReady()
        {
            return _filledBit && _filledAsk;
        }
    }
}
