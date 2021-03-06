using System;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Cache
{

    public static class MemCache
    {
        public static MemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());

        public static void Save(string key, string value, TimeSpan offset) => MemoryCache.Set(key, value, new MemoryCacheEntryOptions().SetAbsoluteExpiration(offset));

        public static void Save(string key, string value) => Save(key, value, TimeSpan.FromDays(1));

        public static bool TryGetValue(string key, out string value) => MemoryCache.TryGetValue(key, out value);

        public static void Remove(string key) => MemoryCache.Remove(key);

        public static void Refresh()
        {
            MemoryCache.Dispose();

            MemoryCache = new MemoryCache(new MemoryCacheOptions());
        }
    }
}