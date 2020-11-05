using Autofac;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Host;
using Microservice.Workflow.Modules;
using Microsoft.Owin;
using Owin;
using Metrics;
using Owin.Metrics;
using System.Web.Http;
using IntelliFlo.Platform.Health;

[assembly: OwinStartup(typeof(ApiStartup))]
namespace Microservice.Workflow.Host
{
    public class ApiStartup : DefaultApiStartup
    {
        protected override void SetupContainer(IAppBuilder app, ContainerBuilder builder)
        {
            base.SetupContainer(app, builder);
            builder.RegisterModule(new WebApiETagFunctionModule(LifeTimeScopes.All));
            builder.RegisterModule(new WorkflowAutofacModule(LifeTimeScopes.All));
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
