using System;
using System.Diagnostics;
using System.Linq;
using RFI.MicroserviceFramework._Api;
using RFI.MicroserviceFramework._Loggers;

namespace RFI.MicroserviceFramework._Environment
{
    public static class SEnv
    {
        static SEnv()
        {
            try
            {
                Vault.Init();
            }
            catch(Exception ex)
            {
                ApiHealth.Set("env", false);
                ex.Log();
            }
        }


        public static string Get(string name) => Environment.GetEnvironmentVariable(name);

        public static string EnvironmentName => Get("ASPNETCORE_ENVIRONMENT");

        public static readonly bool IsDebug = Debugger.IsAttached || AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("xunit"));

        public static string Hostname => Get("HOSTNAME") ?? "Hostname not set";

        public static string SsoKeyPublic => Get("SSO_KEY_PUBLIC");

        public static string RfiKeyPrivate => Get("RFI_KEY_PRIVATE");

        public static string RfiKeyPublic => Get("RFI_KEY_PUBLIC");

        public static string OracleCS => Get("ORACLE_CS");


        public static bool IsDevelopment => EnvironmentName == "Development";
    }
}