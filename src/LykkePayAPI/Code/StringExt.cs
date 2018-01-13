using System.Text.RegularExpressions;

namespace LykkePay.API.Code
{
    public static class StringExt
    {
        private static readonly Regex TrimSplashes = new Regex(@"(?<!\:)[\/\\]+");
        public static string TrimDoubleSplash(this string str)
        {
            return TrimSplashes.Replace(str, @"/");
        }
    }
}
