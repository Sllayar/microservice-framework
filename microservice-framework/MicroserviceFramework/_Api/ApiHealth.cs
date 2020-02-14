using System;
using System.Collections.Generic;
using System.Linq;
using RFI.MicroserviceFramework._Helpers;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Api
{
    internal class ApiHealthItem
    {
        public string Name { get; set; }

        private bool? result;
        public bool Result
        {
            get
            {
                if(result.NotNull()) return result == true;
                if(Timeout.NotNull()) return (DateTime.Now - LastCheck).TotalMilliseconds > Timeout;
                return true;
            }
            set => result = value;
        }

        public DateTime LastCheck { get; set; } = DateTime.Now;

        public int? Timeout { get; set; }
    }


    public static class ApiHealth
    {
        private static readonly Dictionary<string, ApiHealthItem> HealthItems = new Dictionary<string, ApiHealthItem>();

        public static void Set(string name, bool result)
        {
            if(HealthItems.ContainsKey(name).Not()) HealthItems.Add(name, new ApiHealthItem { Name = name });
            HealthItems[name].Result = result;
        }

        public static void Set(string name, DateTime lastCheck, int timeout)
        {
            if(HealthItems.ContainsKey(name).Not()) HealthItems.Add(name, new ApiHealthItem { Name = name });
            HealthItems[name].LastCheck = lastCheck;
            HealthItems[name].Timeout = timeout;
        }

        internal static IEnumerable<string> Check() => HealthItems.Where(item => item.Value.Result.Not()).Select(item => item.Value.Name);
    }
}