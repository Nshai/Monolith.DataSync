using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Configuration;
using Autofac.Core;
using IntelliFlo.Platform;
using Microservice.Workflow.DataProfiles;
using Microservice.Workflow.Modules;
using Microservice.Workflow.v1.Activities;

namespace Microservice.Workflow.Host
{
    public class ContainerStartup : DefaultContainerStartup
    {
        public ContainerStartup(IMicroServiceSettings settings) : base(settings) {}

        public override void InitialiseContainer(ContainerBuilder builder)
        {
            base.InitialiseContainer(builder);

            builder.RegisterType<DevInitialiser>().InstancePerDependency();
            builder.RegisterType<SubsysInitialiser>().InstancePerDependency();
            builder.RegisterType<WorkflowMigrationProfileInitializer>().InstancePerDependency();

            builder.RegisterModule(new ConfigurationSettingsReader());
            builder.RegisterModule(new WorkflowAutofacModule());
        }
    }
}
