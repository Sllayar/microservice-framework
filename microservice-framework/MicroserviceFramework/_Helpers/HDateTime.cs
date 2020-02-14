using System;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HDateTime
    {
        public static DateTime ParseDateTime(this string source)
        {
            DateTime.TryParse(source, out var resultDateTime);
            return resultDateTime;
        }
    }
}