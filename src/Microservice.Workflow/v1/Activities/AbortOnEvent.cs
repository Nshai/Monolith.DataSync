using System.Activities;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Policy;
using Microservice.Workflow.Collaborators.v1;
using Constants = Microservice.Workflow.Engine.Constants;

namespace Microservice.Workflow.v1.Activities
{
    public sealed class AbortOnEvent : NativeActivity
    {
        public InArgument<string> EventType { get; set; }
        public InArgument<int> EntityId { get; set; }
        public InArgument<string> Filter { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var workflowContext = (WorkflowContext)context.Properties.Find(WorkflowConstants.WorkflowContextKey);

            var eventType = EventType.Get(context);
            var entityId = EntityId.Get(context);
            var filter = Filter.Get(context);

            using (UserContextBuilder.FromBearerToken(workflowContext.BearerToken))
            {
                var serviceRegistry = IoC.Resolve<IServiceAddressRegistry>(Constants.ContainerId);
                var clientConfiguration = serviceRegistry.GetServiceEndpoint("workflow");

                this.LogMessage(context, LogLevel.Info, "Subscribe to event {0} for entity {1}", eventType, entityId);
                var abortUri = string.Format("{0}/{1}", clientConfiguration.BaseAddress.TrimEnd('/'), string.Format(Uris.Self.AbortInstance, context.WorkflowInstanceId));

                var clientFactory = IoC.Resolve<IHttpClientFactory>(Constants.ContainerId);
                using (var workflowClient = clientFactory.Create("eventmanagement"))
                {
                    var subscribeTask = workflowClient.UsingPolicy(HttpClientPolicy.Retry).SendAsync(c => c.Post<EventSubscriptionDocument, SubscribeRequest>(Uris.EventManagement.Post, new SubscribeRequest()
                    {
                        EventType = eventType,
                        EntityId = entityId,
                        Filter = filter,
                        CallbackUrl = abortUri,
                        IsPersistent = false
                    })).ContinueWith(t =>
                    {
                        t.OnException(s => { throw new HttpClientException(s); });
                    });
                    subscribeTask.Wait();
                }
            }
        }
    }
}
