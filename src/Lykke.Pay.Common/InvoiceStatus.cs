using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Pay.Common
{
    public enum InvoiceStatus
    {
        Draft,
        Paid,
        Unpaid,
        Removed,
        Underpaid,
        Overpaid,
        LatePaid

    }
}
