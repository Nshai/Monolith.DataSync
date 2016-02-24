using System.Activities;
using System.Net;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Policy;
using Microservice.Workflow.Collaborators.v1;

namespace Microservice.Workflow.v1.Activities
{
    public class LeadTaskBuilder : EntityTaskBuilder
    {
        public LeadTaskBuilder(IHttpClientFactory clientFactory, Activity parentActivity, NativeActivityContext context) : base(clientFactory, parentActivity, context) { }

        public async override Task<int> GetContextPartyId(string ownerContextRole, WorkflowContext context)
        {
            if (ownerContextRole != "Adviser") 
                return PartyNotFound;
            
            using (var crmClient = ClientFactory.Create("crm"))
            {
                var adviserId = PartyNotFound;
                var leadResponse = await crmClient.UsingPolicy(HttpClientPolicy.Retry).SendAsync(c => c.Get<LeadDocument>(string.Format(Uris.Crm.GetLead, context.EntityId)));
                await leadResponse.OnNotFound(async () =>
                {
                    var clientResponse = await crmClient.Get<ClientDocument>(string.Format(Uris.Crm.GetClient, context.EntityId));
                    clientResponse.OnException(s => { throw new HttpClientException(s); });

                    var client = clientResponse.Resource;
                    adviserId = client.CurrentAdviserPartyId;
                });

                if (adviserId != PartyNotFound)
                    return adviserId;
                
                leadResponse.OnException(s => { throw new HttpClientException(s); });
                var lead = leadResponse.Resource;
                return lead.CurrentAdviserPartyId.HasValue ? lead.CurrentAdviserPartyId.Value : PartyNotFound;
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