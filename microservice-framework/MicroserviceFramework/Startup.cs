using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using RFI.MicroserviceFramework._Loggers;

// ReSharper disable All

namespace RFI.MicroserviceFramework
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            app.UseMetricServer();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            lifetime.ApplicationStarted.Register(OnStarted);
            lifetime.ApplicationStopping.Register(OnShutdown);
        }

        private static void OnStarted()
        {
            Logger.Log(true, "Microservice started");

            MicroserviceFramework.Settings.OnStarted();
        }

        private static void OnShutdown()
        {
            MicroserviceFramework.Settings.OnShutdown();

            Logger.Log(true, "Microservice is shutting down");
        }
    }
}