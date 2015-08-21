using System.Activities;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using Microservice.Workflow.Collaborators.v1;

namespace Microservice.Workflow.v1.Activities
{
    public class ClientTaskBuilder : EntityTaskBuilder
    {
        public ClientTaskBuilder(IServiceHttpClientFactory clientFactory, Activity parentActivity, NativeActivityContext context) : base(clientFactory, parentActivity, context) { }

        public async override Task<int> GetContextPartyId(string ownerContextRole, WorkflowContext context)
        {
            if (ownerContextRole != "ServicingAdviser") 
                return PartyNotFound;

            using (var crmClient = ClientFactory.Create("crm"))
            {
                var clientResponse = await crmClient.Get<ClientDocument>(string.Format(Uris.Crm.GetClient, context.EntityId));
                clientResponse.OnException(s => { throw new HttpClientException(s); });
                var client = clientResponse.Resource;

                return client.CurrentAdviserPartyId;
            }
        }

        public async override Task<int> PrepareRequest(CreateTaskRequest request, WorkflowContext context)
        {
            request.RelatedPartyId = context.EntityId;
            if (context.RelatedEntityId > 0)
                request.RelatedJointPartyId = context.RelatedEntityId;
            return context.EntityId;
        }
    }
}