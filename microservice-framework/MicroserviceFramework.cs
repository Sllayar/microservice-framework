﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
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

            SEnv.Init(settings.IsDebug);

            Host.CreateDefaultBuilder(settings.Args).ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>()).Build().Run();
        }
    }

    public class MicroserviceFrameworkSettings
    {
        public string[] Args { get; set; }

        public bool IsDebug { get; set; }

        public bool CreateDefaultBuilder { get; set; }

        public Action OnStarted { get; set; } = () => { };

        public Action OnShutdown { get; set; } = () => { };
    }
}