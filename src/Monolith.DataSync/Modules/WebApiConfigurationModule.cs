using Autofac;

namespace Microservice.DataSync.Modules
{
    public class WebApiConfigurationModule : Module
    {
        private readonly object[] lifeTimeScopeTags;

        public WebApiConfigurationModule(params object[] lifeTimeScopeTags)
        {
            this.lifeTimeScopeTags = lifeTimeScopeTags;
        }

        protected override void Load(ContainerBuilder builder)
        {
        }
    }
}
