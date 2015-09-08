using System.Configuration;
using Autofac;
using Microservice.Workflow.Domain;
using Microservice.Workflow.Engine;
using Microservice.Workflow.Engine.Impl;
using Microservice.Workflow.v1;
using Microservice.Workflow.v1.Activities;
using Microservice.Workflow.v1.Resources;

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
                .As<IWorkflowHost>();

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