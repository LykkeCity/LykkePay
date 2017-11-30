using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LykkePay.API.Code
{
    public static class StringExt
    {
        private static readonly string StorageFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private static readonly IFormatProvider Provider = CultureInfo.InvariantCulture;
        public static string ToUnixFormat(this string str)
        {
            try
            {
                var dateResult = DateTime.ParseExact(str, StorageFormat, Provider);
                var seconds = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                return seconds.ToString(Provider);
            }
            catch
            {
                return "0";
            }
            
        }
    }
}
