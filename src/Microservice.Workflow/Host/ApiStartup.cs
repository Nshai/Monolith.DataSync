using Autofac;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Host;
using Microservice.Workflow.Modules;
using Microsoft.Owin;
using Owin;

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
    }
}