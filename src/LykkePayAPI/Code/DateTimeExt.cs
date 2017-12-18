using System;
using System.Globalization;

namespace LykkePay.API.Code
{
    public static class DateTimeExt
    {
        public static string RepoDateStr(this DateTime date)
        {
            return date.ToUniversalTime().ToString(CultureInfo.InvariantCulture);
        }


    }
}