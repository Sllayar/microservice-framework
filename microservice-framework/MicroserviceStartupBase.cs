using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using RFI.MicroserviceFramework._Loggers;

// ReSharper disable All

namespace RFI
{
    public class MicroserviceStartupBase
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            app.UseMetricServer();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            lifetime.ApplicationStarted.Register(OnStarted);
            lifetime.ApplicationStarted.Register(OnMicroserviceStarted);

            lifetime.ApplicationStopping.Register(OnShutdown);
            lifetime.ApplicationStopping.Register(OnMicroserviceShutdown);
        }

        private static void OnMicroserviceStarted()
        {
            Logger.Log(true, "Microservice started");
        }

        private static void OnMicroserviceShutdown()
        {
            Logger.Log(true, "Microservice is shutting down");
        }

        public virtual void OnStarted()
        {
        }

        public virtual void OnShutdown()
        {
        }
    }
}