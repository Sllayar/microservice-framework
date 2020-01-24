// ReSharper disable All

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HInt
    {
        public static int ParseInt(this object obj, int def = 0) => int.TryParse(obj.ToString(), out var tryResult) ? tryResult : def;
    }
}