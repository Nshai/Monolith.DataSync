using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Configuration;
using Autofac.Core;
using Common.Logging;
using IntelliFlo.Platform.Caching;
using IntelliFlo.Platform.Client;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Impl;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.Identity.Impl;
using IntelliFlo.Platform.NHibernate;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Security;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.Engine;
using IntelliFlo.Platform.Services.Workflow.Modules;
using IntelliFlo.Platform.Services.Workflow.v1.Activities;
using NHibernate;
using Constants = IntelliFlo.Platform.Services.Workflow.Engine.Constants;

namespace IntelliFlo.Platform.Services.Workflow.Host
{
    public class WorkflowStartup : BaseStartup
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public WorkflowStartup(IMicroServiceSettings microServiceSettings) : base(microServiceSettings) {}
        
        public override IDisposable Start()
        {
            SetupContainer();
            InitialiseWorkflows();
            
            Console.WriteLine(" ROLE:    Workflow");

            return this;
        }

        public override void Dispose()
        {
            IoC.Reset(Constants.ContainerId);
        }

        private static void InitialiseWorkflows()
        {
            IWorkflowHost host;
            if(!Platform.IoC.Container.TryResolve(out host))
                return;

            var sessionFactory = IoC.Resolve<ISessionFactory>(Constants.ContainerId);
            using (var session = sessionFactory.OpenSession())
            {
                var instanceStepRepository = new NHibernateRepository<InstanceHistory>(session);
                var instanceRepository = new NHibernateRepository<Instance>(session);

                var pausedInstanceIds = (from step in instanceStepRepository.Query()
                    where step.IsComplete == false && step.Step == StepName.Delay.ToString()
                    select step.InstanceId).ToList();

                var templateGroups = instanceRepository.Query().Where(i => pausedInstanceIds.Contains(i.Id)).Select(i => i.Template);

                foreach (var template in templateGroups)
                {
                    log.InfoFormat("Initialising template {0}", template.Id);
                    try
                    {
                        host.Initialise(template);
                    }
                    catch (Exception ex)
                    {
                        log.WarnFormat("Failed to initialise template {0}", ex, template.Id);
                    }
                }
            }
        }

        private static void SetupContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationSettingsReader());
            builder.RegisterModule(new WorkflowAutofacModule());
            builder.RegisterModule(new AutofacSecurityModule());

            builder.RegisterType<EntityTaskBuilderFactory>().As<IEntityTaskBuilderFactory>();

            #region Client

            builder.Register(c => new FuncMutator(x => x))
                .As<IHttpClientConfigurationMutator>();

            builder.RegisterType<ServiceHttpClientFactory>()
                .As<IServiceHttpClientFactory>();

            builder.Register(c => ServiceAddressRegistrySection.GetSection())
               .As<IServiceAddressRegistry>()
               .SingleInstance();

            builder.RegisterType<DefaultHttpClientConfiguration>()
                .As<IHttpClientConfiguration>();

            builder.RegisterType<ConfigureHalFormatters>()
                .As<IHttpClientConfigurationMutator>();

            builder.RegisterType<ConfigureTrustedClientAuth>()
                .As<IHttpClientConfigurationMutator>();

            #endregion

            #region Security

            builder.RegisterType<TrustedClientAuthenticationTokenBuilder>()
                .As<ITrustedClientAuthenticationTokenBuilder>();

            builder.RegisterType<CertificateProviderFactory>()
                .As<ICertificateProviderFactory>();

            builder.RegisterType<SignAuthenticationMessageBuilder>()
                .As<ISignAuthenticationMessageBuilder>();

            builder.RegisterType<SignManager>().As<ISignManager>();

            #endregion

            switch (ConfigurationManager.AppSettings["cache.use"])
            {
                case "memcached":
                    builder
                        .RegisterType<EnyimCache>()
                        .As<ICache>()
                        .SingleInstance();
                    break;
                case "nullcache":
                    builder
                        .RegisterType<NullCache>()
                        .As<ICache>()
                        .SingleInstance();
                    break;
                default:
                    throw new ConfigException("cache.use configuration set to unknown value");
            }

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("AutoMapperModule"))
                .As<IModule>()
                .SingleInstance();

            builder.Register(c =>
            {
                var provider = c.Resolve<IReadWriteSessionFactoryProvider>();
                return provider.SessionFactory;
            }).As<ISessionFactory>();

            var container = builder.Build();

            IoC.Initialize(Constants.ContainerId, container);
        }
    }
}
