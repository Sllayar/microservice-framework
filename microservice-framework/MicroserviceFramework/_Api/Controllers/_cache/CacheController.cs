using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RFI.MicroserviceFramework._Cache;
using RFI.MicroserviceFramework._Loggers;

namespace RFI.MicroserviceFramework._Api.Controllers._cache
{
    public class DeleteValueRequest
    {
        public string Key { get; set; }
    }

    [Route("CacheController")]
    public class CacheController : ControllerBase
    {
        [HttpGet]
        [Route("GetCaheCount")]
        public ContentResult clearCache() => Content(MemCache.MemoryCache.Count.ToString());

        [HttpGet]
        [Route("clearAll")]
        public ContentResult GetCacheValues()
        {
            try
            {
                MemCache.Refresh();

                Logger.Log(true, "Cache Clear", null, "clearCache/all");
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "clearCache/DeleteValue fail",
                    "RFI.MicroserviceFramework._Api.Controllers._cache.ClearByValue");

                return Content(ex.ToString());
            }

            return Content("Success");
        }

        [HttpPost]
        [Route("DeleteValue")]
        public ContentResult ClearByValue([FromBody]DeleteValueRequest Request)
        {
            try
            {
                if (!String.IsNullOrEmpty(Request.Key) && MemCache.TryGetValue(Request.Key, out var cacheValue))
                {
                    MemCache.Remove(Request.Key);

                    return Content("Success");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "clearCache/DeleteValue fail",
                    "RFI.MicroserviceFramework._Api.Controllers._cache.ClearByValue");

                return Content(ex.ToString());
            }

            return Content("Value not find");
        }
    }
}
