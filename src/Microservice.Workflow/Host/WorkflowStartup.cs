﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using Autofac;
using Autofac.Configuration;
using Autofac.Core;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Caching;
using IntelliFlo.Platform.Client;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Impl;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.Identity.Impl;
using IntelliFlo.Platform.NHibernate;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Security;
using log4net;
using Microservice.Workflow.Domain;
using Microservice.Workflow.Engine;
using Microservice.Workflow.Modules;
using Microservice.Workflow.v1.Activities;
using NHibernate;
using Constants = Microservice.Workflow.Engine.Constants;
using Parameter = IntelliFlo.Platform.NHibernate.Repositories.Parameter;

namespace Microservice.Workflow.Host
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
            try
            {
                IWorkflowHost host;
                if (!IntelliFlo.Platform.IoC.Container.TryResolve(out host))
                    return;

                host.Dispose();

                IoC.Reset(Constants.ContainerId);
            }
            catch (Exception ex)
            {
                log.Error("Exception occurred whilst shutting down workflow host", ex);
            }
        }

        private static void InitialiseWorkflows()
        {
            IWorkflowHost host;
            if(!IntelliFlo.Platform.IoC.Container.TryResolve(out host))
                return;

            var sessionFactory = IoC.Resolve<ISessionFactory>(Constants.ContainerId);
            using (var session = sessionFactory.OpenSession())
            {
                var templateRepository = new NHibernateRepository<TemplateDefinition>(session);
                var templates = templateRepository.ReportAll<TemplateDefinition>("GetDelayedTemplates", new Parameter("Version", TemplateDefinition.DefaultVersion));

                foreach (var template in templates)
                {
                    log.InfoFormat("Initialising template {0}", template.Id);
                    try
                    {
                        host.Initialise(template);
                    }
                    catch (Exception ex)
                    {
                        log.Warn($"Failed to initialise template {template.Id}", ex);
                    }
                }
            }
        }

        private static void SetupContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationSettingsReader());
            builder.RegisterModule(new WorkflowAutofacModule());

            builder.RegisterType<EntityTaskBuilderFactory>().As<IEntityTaskBuilderFactory>();

            #region Client

            builder.RegisterModule(new AutofacHttpClientModule());

            builder.Register(c => new DefaultHttpClientFactory(c.Resolve<IServiceAddressRegistry>(), client => client.ResponseFormatters.Add(new JsonMediaTypeFormatter()), () => new [] { new TrustedClientAuthenticationDelegatingHandler() }))
                .As<IHttpClientFactory>()
                .InstancePerDependency();

            builder.Register(c => ServiceAddressRegistrySection.GetSection())
               .As<IServiceAddressRegistry>()
               .SingleInstance();

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


            var hostConfiguration = new ConfigureNHibernateForMsSql2005("host", new AssemblyScanner().AssembliesToScan());

            builder.Register(c => new NHibernateSessionFactoryProvider(
                                   hostConfiguration,
                                   c.Resolve<IEnumerable<INHibernateInitializationAware>>()))
               .As<IHostSessionFactoryProvider>()
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
