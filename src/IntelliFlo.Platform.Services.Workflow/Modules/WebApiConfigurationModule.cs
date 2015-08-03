using Autofac;
using IntelliFlo.Platform.Http.Serialization;

namespace IntelliFlo.Platform.Services.Workflow.Modules
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
            builder.RegisterType<PdfDocumentGeneratorResolver>()
               .As<IPdfDocumentGeneratorResolver>()
               .SingleInstance();
        }
    }
}