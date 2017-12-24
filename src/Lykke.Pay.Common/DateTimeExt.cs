using System;
using System.Globalization;

namespace Lykke.Pay.Common
{
    public static class DateTimeExt
    {
        public static string RepoDateStr(this DateTime date)
        {
            return date.ToUniversalTime().ToString(CultureInfo.InvariantCulture);
        }


    }
}