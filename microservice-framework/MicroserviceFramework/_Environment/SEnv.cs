using System;
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
                //Healthes.Set("env", false);
                ex.Log();
            }
        }


        public static string Get(string name) => Environment.GetEnvironmentVariable(name);

        public static string EnvironmentName => Get("ASPNETCORE_ENVIRONMENT");

        public static readonly bool IsDebug = System.Diagnostics.Debugger.IsAttached;

        public static string Hostname => SEnv.Get("HOSTNAME") ?? "Hostname not set";

        public static string SsoKeyPublic => SEnv.Get("SSO_KEY_PUBLIC");

        public static string RfiKeyPrivate => SEnv.Get("RFI_KEY_PRIVATE");

        public static string RfiKeyPublic => SEnv.Get("RFI_KEY_PUBLIC");

        public static string OracleCS => SEnv.Get("ORACLE_CS");


        public static bool IsDevelopment => SEnv.EnvironmentName == "Development";
    }
}