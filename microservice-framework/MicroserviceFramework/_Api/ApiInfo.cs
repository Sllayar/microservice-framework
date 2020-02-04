using System.Collections.Generic;
using System.Reflection;
using RFI.MicroserviceFramework._Environment;

namespace RFI.MicroserviceFramework._Api
{
    public static class ApiInfo
    {
        public static Dictionary<string, string> Get() => new Dictionary<string, string>
        {
            { "Environment", SEnv.EnvironmentName },
            { "Configuration", SEnv.IsDebug ? "Debug" : "Release" },
            { "PackageName", ApiDbController.PackageName },
            { "Hostname", SEnv.Hostname },
            { "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString() }
        };
    }
}