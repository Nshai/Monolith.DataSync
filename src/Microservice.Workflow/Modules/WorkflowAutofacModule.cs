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
using Microservice.Workflow.Utilities.TimeZone;
using Microservice.Workflow.v1;
using Microservice.Workflow.v1.Activities;
using Microservice.Workflow.v1.Resources;
using NodaTime;
using NodaTime.TimeZones;
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
            builder.RegisterType<EntityTaskBuilderFactory>().As<IEntityTaskBuilderFactory>();

            // Register auto-mappers
            // TODO - isnt this called automatically by some other container?
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
             .Where(t => t.Name.EndsWith("AutoMapperModule"))
             .As<IModule>()
             .SingleInstance();

            //Register Timezone infrastructure
            builder.RegisterType<TimeZoneConverter>().As<ITimeZoneConverter>().InstancePerMatchingLifetimeScope(lifeTimeScopeTags);
            builder.Register(c => BuildDateTimeZoneProvider()).As<IDateTimeZoneProvider>().InstancePerMatchingLifetimeScope(lifeTimeScopeTags);
        }

        public static IDateTimeZoneProvider BuildDateTimeZoneProvider()
        {
            IDateTimeZoneProvider provider;
            var assembly = Assembly.GetAssembly(typeof(TimeZoneConverter));
            using (var stream = assembly
                .GetManifestResourceStream($"{typeof(TimeZoneConverter).Namespace}.timezoneinfo.nzd"))
            {
                var source = TzdbDateTimeZoneSource.FromStream(stream);
                provider = new DateTimeZoneCache(source);
            }

            return provider;
        }
    }
}