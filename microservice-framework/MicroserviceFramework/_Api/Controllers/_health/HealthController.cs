using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RFI.MicroserviceFramework._Helpers;
using RFI.MicroserviceFramework._Loggers;
using RFI.MicroserviceFramework._Metrics;

namespace RFI.MicroserviceFramework._Api.Controllers._health
{
    public static class Healthes
    {
        private static readonly Dictionary<string, bool> Dict = new Dictionary<string, bool>();

        public static void Set(string name, bool healthcheck)
        {
            if(Dict.ContainsKey(name).Not()) Dict.Add(name, healthcheck);
            else Dict[name] = healthcheck;
        }

        public static IEnumerable<string> Check() => Dict.Where(h => h.Value.Not()).Select(h => h.Key);
    }

    [Route("health")]
    [Route("health/{probe}")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public string HealthProbe(string probe = "health")
        {
            var fails = Healthes.Check();

            var success = fails.Any().Not();

            SMetrics.CounterHealthCheck.Inc(success);

            if(success.Not())
            {
                HttpContext.Response.StatusCode = 500;
                var response = $"Health check failed [{probe}][{fails.Aggregate("", (current, fail) => $"{current}, {fail}")}]";
                Logger.Log(false, response);
                return response;
            }

            HttpContext.Response.StatusCode = 200;
            return "Health check success";
        }
    }
}