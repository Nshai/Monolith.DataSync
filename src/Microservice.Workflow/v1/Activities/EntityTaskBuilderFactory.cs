using System;
using System.Activities;
using IntelliFlo.Platform.Http.Client;

namespace Microservice.Workflow.v1.Activities
{
    public class EntityTaskBuilderFactory : IEntityTaskBuilderFactory
    {
        private readonly IHttpClientFactory clientFactory;

        public EntityTaskBuilderFactory(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        public EntityTaskBuilder Get(string type, Activity parentActivity, NativeActivityContext context)
        {
            switch (type)
            {
                case "Client":
                    return new ClientTaskBuilder(clientFactory, parentActivity, context);
                case "Plan":
                    return new PlanTaskBuilder(clientFactory, parentActivity, context);
                case "Adviser":
                    return new AdviserTaskBuilder(clientFactory, parentActivity, context);
                case "ServiceCase":
                    return new ServiceCaseTaskBuilder(clientFactory, parentActivity, context);
                case "Lead":
                    return new LeadTaskBuilder(clientFactory, parentActivity, context);
                default:
                    throw new ArgumentException(string.Format("Unknown entity type {0}", type));
            }
        }
    }
}