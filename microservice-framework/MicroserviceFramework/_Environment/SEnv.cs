using System;
using RFI.MicroserviceFramework._Api.Controllers._health;
using RFI.MicroserviceFramework._Loggers;

namespace RFI.MicroserviceFramework._Environment
{
    public static class SEnv
    {
        public static void Init(bool isDebug)
        {
            try
            {
                IsDebug = isDebug;

                Vault.Init();
            }
            catch(Exception ex)
            {
                Healthes.Set("env", false);
                ex.Log();
            }
        }


        public static string Get(string name) => Environment.GetEnvironmentVariable(name);


        public static string EnvironmentName => Get("ASPNETCORE_ENVIRONMENT");

        public static string Hostname => Get("HOSTNAME") ?? "Hostname not set";

        public static string SsoKeyPublic => Get("SSO_KEY_PUBLIC");

        public static string RfiKeyPrivate => Get("RFI_KEY_PRIVATE");

        public static string RfiKeyPublic => Get("RFI_KEY_PUBLIC");

        public static string OracleCS => Get("ORACLE_CS");


        public static bool IsDevelopment => EnvironmentName == "Development";

        public static bool IsDebug { get; private set; }
    }
}