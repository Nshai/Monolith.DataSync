using Autofac;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Host;
using Microservice.Workflow.Modules;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(ApiStartup))]
namespace Microservice.Workflow.Host
{
    public class ApiStartup : DefaultApiStartup
    {
        protected override void SetupContainer(ContainerBuilder builder)
        {
            base.SetupContainer(builder);
            builder.RegisterModule(new WebApiETagFunctionModule(LifeTimeScopes.All));
            builder.RegisterModule(new WorkflowAutofacModule(LifeTimeScopes.All));
        }
    }
}