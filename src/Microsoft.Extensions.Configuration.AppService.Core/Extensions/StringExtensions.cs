namespace Microsoft.Extensions.Configuration.AppService.Core.Extensions
{
    internal static class StringExtensions
    {
        public static string ReplaceSeparator(this string key, string replacement)
        {
            return key.Replace(":", replacement);
        }
    }
}