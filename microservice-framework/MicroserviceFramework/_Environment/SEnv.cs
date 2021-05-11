using System;
using System.Diagnostics;
using System.Linq;
using RFI.MicroserviceFramework._Api;
using RFI.MicroserviceFramework._Helpers;
using RFI.MicroserviceFramework._Loggers;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Environment
{
    public static class SEnv
    {
        static SEnv()
        {
            try
            {
                if(IsDebug.Not() && IsTests.Not()) Vault.Init();
            }
            catch(Exception ex)
            {
                ApiHealth.Set("env", false);
                ex.Log();
            }
        }


        public static string Get(string name) => Environment.GetEnvironmentVariable(name);


        public static string Hostname => Get("HOSTNAME") ?? "Hostname not set";

        public static string EnvironmentName => Get("ASPNETCORE_ENVIRONMENT");

        public static bool IsDevelopment => EnvironmentName == "Development" || EnvironmentName == "Local";

        public static readonly bool IsDebug = Debugger.IsAttached;

        public static readonly bool IsTests = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("xunit"));
    }
}