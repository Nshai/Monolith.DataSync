using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Autofac;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Bus;
using IntelliFlo.Platform.Bus.Scheduler;
using Microservice.Workflow.Host;
using Microservice.Workflow.Messaging.Messages;

namespace Microservice.Workflow.v1.Activities
{
    public sealed class ResumeOnDelay : NativeActivity
    {
        public InArgument<string> Bookmark { get; set; }
        public InArgument<DateTime> DelayUntil { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var workflowContext = (WorkflowContext)context.Properties.Find(WorkflowConstants.WorkflowContextKey);
            var delayUntil = DelayUntil.Get(context);
            var bookmark = Bookmark.Get(context);

            using (var lifetimeScope = IoC.Container.BeginLifetimeScope(WorkflowScopes.Scope))
            using (UserContextBuilder.FromBearerToken(workflowContext.BearerToken, lifetimeScope))
            {
                var busPublisher = lifetimeScope.Resolve<IBusPublisher>();
                var resumeMessage = new ResumeInstance()
                {
                    InstanceId = context.WorkflowInstanceId,
                    Bookmark = bookmark
                };
                var message = new ScheduleTimeout($"WorkflowDelay-{Guid.NewGuid()}", new SimpleSchedule() { StartDate = delayUntil }, resumeMessage)
                {
                    SchedulerVersion = SchedulerVersionEnum.V2
                };
                busPublisher.Publish(message);
            }
        }
    }
}
