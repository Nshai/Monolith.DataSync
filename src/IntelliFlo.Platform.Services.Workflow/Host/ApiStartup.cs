using Autofac;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.Host;
using IntelliFlo.Platform.Services.Workflow.Modules;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(ApiStartup))]
namespace IntelliFlo.Platform.Services.Workflow.Host
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