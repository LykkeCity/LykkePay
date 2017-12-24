using System;
using System.Globalization;

namespace Lykke.Pay.Common
{
    public static class StringExt
    {
        private static readonly IFormatProvider Provider = CultureInfo.InvariantCulture;


        public static DateTime GetRepoDateTime(this string strDate)
        {
            return DateTime.Parse(strDate, Provider).ToLocalTime();
        }


        public static DateTime FromUnixFormat(this string str)
        {
            try
            {
                int seconds = int.Parse(str);
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds).ToLocalTime();

            }
            catch
            {
                return DateTime.Now;
            }

        }
        public static string ToUnixFormat(this string str)
        {
            try
            {
                var dateResult = DateTime.Parse(str, Provider);
                var seconds = (int)(dateResult.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                return seconds.ToString(Provider);
            }
            catch
            {
                return "0";
            }
            
        }

        //MerchantPayRequestStatus<T>
        public static T ParseEnum<T>(this string enumStr) where T : struct, IConvertible, IComparable, IFormattable
        {
            int e;
            T result;
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enum.");
            }

            if (int.TryParse(enumStr, out e))
            {
                result  = (T)(object)e;
            }
            else
            {
                result = (T)Enum.Parse(typeof(T), enumStr);
            }

            return result;
        }
    }
}
