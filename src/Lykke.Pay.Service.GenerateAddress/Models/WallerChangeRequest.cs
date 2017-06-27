using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Pay.Service.GenerateAddress.Models
{
    public class WallerChangeRequest
    {
        public string WalletAddress { get; set; }
        public double DeltaAmount { get; set; }
        public string Assert { get; set; }
    }
}
