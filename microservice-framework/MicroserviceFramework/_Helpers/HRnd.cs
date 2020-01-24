using System;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HRnd
    {
        private static readonly Random rnd = new Random();

        public static int Next(int minValue, int maxValue) => rnd.Next(minValue, maxValue);
    }
}