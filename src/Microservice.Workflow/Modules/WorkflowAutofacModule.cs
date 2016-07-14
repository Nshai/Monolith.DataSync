using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Autofac;
using IntelliFlo.Platform;
using IntelliFlo.Platform.AutoMapper;
using IntelliFlo.Platform.NHibernate;
using Microservice.Workflow.Domain;
using Microservice.Workflow.Engine;
using Microservice.Workflow.Engine.Impl;
using Microservice.Workflow.v1;
using Microservice.Workflow.v1.Activities;
using Microservice.Workflow.v1.Resources;
using Module = Autofac.Module;

namespace Microservice.Workflow.Modules
{
    public class WorkflowAutofacModule : Module
    {
        private readonly object[] lifeTimeScopeTags;

        public WorkflowAutofacModule(params object[] lifeTimeScopeTags)
        {
            this.lifeTimeScopeTags = lifeTimeScopeTags;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WorkflowServiceFactory>()
                .As<IWorkflowServiceFactory>()
                .InstancePerMatchingLifetimeScope(lifeTimeScopeTags);

            builder.RegisterType<WorkflowClientFactory>()
                .As<IWorkflowClientFactory>()
                .SingleInstance();

            builder.RegisterType<WorkflowHost>()
                .As<IWorkflowHost>()
                .SingleInstance();

            var delayInMinutes = ConfigurationManager.AppSettings["workflowDelayInMinutes"];
            if (delayInMinutes != null && delayInMinutes == "true")
            {
                builder.RegisterType<MinuteDelayPeriod>().As<IDelayPeriod>().SingleInstance();
            }
            else
            {
                builder.RegisterType<DayDelayPeriod>().As<IDelayPeriod>().SingleInstance();
            }

            builder.Register(c => (WorkflowConfiguration) ConfigurationManager.GetSection("WorkflowConfiguration"))
                .As<IWorkflowConfiguration>()
                .SingleInstance();

            builder.RegisterType<EventDispatcher>().AsImplementedInterfaces().InstancePerMatchingLifetimeScope(lifeTimeScopeTags);




            // TODO remove once scheduler is instroduced
            var hostConfiguration = new ConfigureNHibernateForMsSql2005("host", new AssemblyScanner().AssembliesToScan());

            builder.Register(c => new NHibernateSessionFactoryProvider(
                                   hostConfiguration,
                                   c.Resolve<IEnumerable<INHibernateInitializationAware>>()))
               .As<IHostSessionFactoryProvider>()
               .SingleInstance();


            builder.RegisterType<EntityTaskBuilderFactory>().As<IEntityTaskBuilderFactory>();

            // Register auto-mappers
            // TODO - isnt this called automatically by some other container?
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
             .Where(t => t.Name.EndsWith("AutoMapperModule"))
             .As<IModule>()
             .SingleInstance();
        }
    }
}