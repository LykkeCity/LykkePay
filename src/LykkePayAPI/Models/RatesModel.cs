using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LykkePay.API.Code;

namespace LykkePay.API.Models
{
    public class RatesModel : BaseReturnModel
    {
        public RatesModel()
        {
            Rates = new List<Rate>();
        }
        public List<Rate> Rates { get; set; }
    }
}
