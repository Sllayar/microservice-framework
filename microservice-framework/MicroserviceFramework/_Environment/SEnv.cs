using System;
using RFI.MicroserviceFramework._Loggers;

namespace RFI.MicroserviceFramework._Environment
{
    public static class SEnv
    {
        public static void Init()
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
    }
}