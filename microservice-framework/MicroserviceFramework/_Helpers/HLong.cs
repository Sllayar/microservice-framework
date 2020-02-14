// ReSharper disable All

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HLong
    {
        public static long ParseLong(this string source)
        {
            long.TryParse(source, out var result);
            return result;
        }
    }
}