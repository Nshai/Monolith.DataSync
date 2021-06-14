using Autofac;
using Autofac.Configuration;
using IntelliFlo.Platform;
using Microservice.DataSync.DataProfiles;
using Microservice.DataSync.Modules;

namespace Microservice.DataSync.Host
{
    public class ContainerStartup : DefaultContainerStartup
    {
        public ContainerStartup(IMicroServiceSettings settings) : base(settings) {}

        public override void InitialiseContainer(ContainerBuilder builder)
        {
            base.InitialiseContainer(builder);

            builder.RegisterType<DevInitialiser>().InstancePerDependency();
            builder.RegisterType<SubsysInitialiser>().InstancePerDependency();
           
            builder.RegisterModule(new ConfigurationSettingsReader());
            builder.RegisterModule(new DataSyncAutofacModule());
        }
    }
}
