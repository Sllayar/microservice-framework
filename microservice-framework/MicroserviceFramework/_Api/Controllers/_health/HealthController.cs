using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RFI.MicroserviceFramework._Helpers;
using RFI.MicroserviceFramework._Loggers;
using RFI.MicroserviceFramework._Metrics;

namespace RFI.MicroserviceFramework._Api.Controllers._health
{
    [Route("health")]
    [Route("health/{probe}")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public string Health()
        {
            HttpContext.Response.StatusCode = 200;
            return "Health check success";
        }

        [HttpGet]
        [Route("HealthProbe")]
        public string HealthProbe(string probe = "manual")
        {
            bool success = ApiHealth.Check(out var fails);

            SMetrics.CounterHealthCheck.Inc(success);

            if(success.Not())
            {
                HttpContext.Response.StatusCode = 500;
                var response = $"Health check ({probe}) failed [{fails.Aggregate("", (current, fail) => $"{current}, {fail}").Trim(' ', ',')}]";
                Logger.Log(false, response);
                return response;
            }

            HttpContext.Response.StatusCode = 200;
            return "Health check success";
        }
    }
}