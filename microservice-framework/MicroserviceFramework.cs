using System;
using RFI.MicroserviceFramework._Environment;

// ReSharper disable All

namespace RFI.MicroserviceFramework
{
    public static class MicroserviceFramework
    {
        public static MicroserviceFrameworkSettings Settings { get; private set; }

        public static void Init(MicroserviceFrameworkSettings settings)
        {
            Settings = settings;

            SEnv.Init();
        }
    }

    public class MicroserviceFrameworkSettings
    {
        public Action OnStarted { get; set; } = () => { };

        public Action OnShutdown { get; set; } = () => { };
    }
}