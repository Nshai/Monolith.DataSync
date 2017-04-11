using System.Activities;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Policy;
using Microservice.Workflow.Collaborators.v1;

namespace Microservice.Workflow.v1.Activities
{
    public class ServiceCaseTaskBuilder : EntityTaskBuilder
    {
        public ServiceCaseTaskBuilder(IHttpClientFactory clientFactory, Activity parentActivity, NativeActivityContext context) : base(clientFactory, parentActivity, context) { }

        public async override Task<int> GetContextPartyId(string ownerContextRole, WorkflowContext context)
        {
            if (ownerContextRole != "Adviser")
                return PartyNotFound;
            
            var serviceCase = await GetServiceCase(context.ClientId, context.EntityId);
            return serviceCase.ServicingAdviserId == 0 ? PartyNotFound : serviceCase.ServicingAdviserId;
        }

        public async override Task<int> PrepareRequest(CreateTaskRequest request, WorkflowContext context)
        {
            var serviceCase = await GetServiceCase(context.ClientId, context.EntityId);

            var owner1Id = serviceCase.ClientId;
            var owner2Id = serviceCase.JointClientId;
            request.RelatedPartyId = context.ClientId;
            request.RelatedServiceCaseId = context.EntityId;

            if (owner2Id != null)
            {
                // NB ClientId may be joint owner (rather than primary)
                var jointOwnerId = owner2Id != context.ClientId ? owner2Id : owner1Id;
                request.RelatedJointPartyId = jointOwnerId;
            }
            return owner1Id;
        }

        private async Task<ServiceCaseDocument> GetServiceCase(int clientId, int serviceCaseId)
        {
            using (var crmClient = ClientFactory.Create("crm"))
            {
                var serviceCaseResponse = await crmClient.UsingPolicy(HttpClientPolicy.Retry).SendAsync(c => c.Get<ServiceCaseDocument>(string.Format(Uris.Crm.GetServiceCase, clientId, serviceCaseId)));
                serviceCaseResponse.OnException(s => { throw new HttpClientException(s); });
                return serviceCaseResponse.Resource;
            }
        }
    }
}