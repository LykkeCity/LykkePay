using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LykkePay.Business
{
    public class BaseRequest
    {
        public string MerchantId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Sign { get; set; }
    }
}
