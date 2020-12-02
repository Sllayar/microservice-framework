using System;
using Microsoft.AspNetCore.Mvc;
using RFI.MicroserviceFramework._Cache;
using RFI.MicroserviceFramework._Loggers;

namespace RFI.MicroserviceFramework._Api.Controllers._cache
{
    public class DeleteValueRequest
    {
        public string Key { get; set; }
    }

    [Route("cache")]
    public class CacheController : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("count")]
        public ContentResult GetCount() => Content(MemCache.MemoryCache.Count.ToString());

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("clear")]
        public ContentResult Clear()
        {
            try
            {
                MemCache.Refresh();

                Logger.Log(true, "Cache Clear", null, "cache/clear");
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "cache/clear fail",
                    "RFI.MicroserviceFramework._Api.Controllers._cache.Clear");

                return Content(ex.ToString());
            }

            return Content("Success");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("delete_value")]
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
                Logger.Log(ex, "delete_value fail",
                    "RFI.MicroserviceFramework._Api.Controllers._cache.ClearByValue");

                return Content(ex.ToString());
            }

            return Content("Value not find");
        }
    }
}
