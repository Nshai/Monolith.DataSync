using System.Security.Cryptography.X509Certificates;
using Autofac;
using IntelliFlo.Platform;
using Microservice.Workflow.DataProfiles;

namespace Microservice.Workflow.Host
{
    public class ContainerStartup : DefaultContainerStartup
    {
        public ContainerStartup(IMicroServiceSettings settings) : base(settings) {}

        public override void InitialiseContainer(ContainerBuilder builder)
        {
            base.InitialiseContainer(builder);

            builder.RegisterType<DevInitialiser>().InstancePerDependency();
        }
    }

}
