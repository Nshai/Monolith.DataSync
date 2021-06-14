using System.Web.Http;
using Autofac;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using Metrics;
using Microservice.DataSync.Host;
using Microservice.DataSync.Modules;
using Microsoft.Owin;
using Owin;
using Owin.Metrics;

[assembly: OwinStartup(typeof(ApiStartup))]
namespace Microservice.DataSync.Host
{
    public class ApiStartup : DefaultApiStartup
    {
        protected override void SetupContainer(IAppBuilder app, ContainerBuilder builder)
        {
            base.SetupContainer(app, builder);
            builder.RegisterModule(new DataSyncAutofacModule(LifeTimeScopes.All));
        }

        protected override void SetupOwinMiddleWare(IAppBuilder app, HttpConfiguration config)
        {
      
            Metric.Config
            .WithOwin(middleware => app.Use(middleware), cfg => cfg
                .WithRequestMetricsConfig(c => c.WithAllOwinMetrics())
                .WithMetricsEndpoint(endpointConfig => endpointConfig
                    .MetricsHealthEndpoint("healthM")
                    .MetricsPingEndpoint("ping")
                    .MetricsJsonEndpoint("json")
                    .MetricsTextEndpoint("text")
                    .MetricsEndpoint()));


            base.SetupOwinMiddleWare(app, config);
        }
    }
}
