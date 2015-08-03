using Autofac;

namespace IntelliFlo.Platform.Services.Workflow.Modules
{
    public class WebApiETagFunctionModule : Module
    {
        private readonly object[] lifeTimeScopeTags;

        public WebApiETagFunctionModule(params object[] lifeTimeScopeTags)
        {
            this.lifeTimeScopeTags = lifeTimeScopeTags;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // REGISTER YOUR ETAG FUNCTIONS HERE:

            //builder.RegisterType<NHibernateChecksumProvider<FactFind>>()
            //    .Named<IResourceChecksumProvider>(ETagConstants.Functions.FactFind)
            //    .InstancePerRequest();
        }
    }
}