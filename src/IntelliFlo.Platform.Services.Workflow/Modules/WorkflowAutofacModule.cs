using System.Configuration;
using Autofac;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.Engine;
using IntelliFlo.Platform.Services.Workflow.Engine.Impl;
using IntelliFlo.Platform.Services.Workflow.v1;
using IntelliFlo.Platform.Services.Workflow.v1.Activities;
using IntelliFlo.Platform.Services.Workflow.v1.Resources;
using Module = Autofac.Module;

namespace IntelliFlo.Platform.Services.Workflow.Modules
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
        }
    }
}