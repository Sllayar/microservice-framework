using System.Globalization;

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HDecimal
    {
        public static decimal ToDecimal(this string source)
        {
            decimal.TryParse(source.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result);
            return result;
        }
    }
}