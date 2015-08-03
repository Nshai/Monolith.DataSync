﻿using System.Activities;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Services.Workflow.Collaborators.v1;
using IntelliFlo.Platform.Services.Workflow.Engine;
using IntelliFlo.Platform.Services.Workflow.Host;
using log4net;
using Constants = IntelliFlo.Platform.Services.Workflow.Engine.Constants;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public sealed class AbortOnEvent : NativeActivity
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(AbortOnEvent));

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

                logger.InfoFormat("Subscribe to event {0} for entity {1}", eventType, entityId);
                var abortUri = string.Format("{0}/{1}", clientConfiguration.BaseAddress.TrimEnd('/'), string.Format(Uris.Self.AbortInstance, context.WorkflowInstanceId));

                var clientFactory = IoC.Resolve<IServiceHttpClientFactory>(Constants.ContainerId);
                using (var workflowClient = clientFactory.Create("eventmanagement"))
                {
                    var subscribeTask = workflowClient.Post<EventSubscriptionDocument, SubscribeRequest>(Uris.EventManagement.Post, new SubscribeRequest()
                    {
                        EventType = eventType,
                        EntityId = entityId,
                        Filter = filter,
                        CallbackUrl = abortUri,
                        IsPersistent = false
                    }).ContinueWith(t =>
                    {
                        t.OnException(s => { throw new HttpClientException(s); });
                    });
                    subscribeTask.Wait();
                }
            }
        }
    }
}
