using System.Activities;
using System.Linq;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using Microservice.Workflow.Collaborators.v1;

namespace Microservice.Workflow.v1.Activities
{
    public class PlanTaskBuilder : EntityTaskBuilder
    {
        public PlanTaskBuilder(IServiceHttpClientFactory clientFactory, Activity parentActivity, NativeActivityContext context) : base(clientFactory, parentActivity, context) { }

        public async override Task<int> GetContextPartyId(string ownerContextRole, WorkflowContext context)
        {
            var plan = await GetPlan(context.ClientId, context.EntityId);
            
            switch (ownerContextRole)
            {
                case "SellingAdviser":
                    return plan.SellingAdviserPartyId.HasValue ? plan.SellingAdviserPartyId.Value : 0;
                case "ServicingAdministrator":
                    return plan.ServicingAdministratorPartyId.HasValue ? plan.ServicingAdministratorPartyId.Value : 0;
                case "TandCCoach":
                    return plan.TnCCoachPartyId.HasValue ? plan.TnCCoachPartyId.Value : 0;
                default:
                    return PartyNotFound;
            }
        }

        public async override Task<int> PrepareRequest(CreateTaskRequest request, WorkflowContext context)
        {
            var plan = await GetPlan(context.ClientId, context.EntityId);

            var owner1 = plan.Owners.First();
            var owner2 = plan.Owners.ElementAtOrDefault(1);
            request.RelatedPartyId = context.ClientId;
            request.RelatedPlanId = context.EntityId;
            if (owner2 != null)
                request.RelatedJointPartyId = owner2.ClientId;

            return owner1.ClientId;
        }

        private async Task<PlanDocument> GetPlan(int clientId, int planId)
        {
            using (var portfolioClient = ClientFactory.Create("portfolio"))
            {
                var planResponse = await portfolioClient.Get<PlanDocument>(string.Format(Uris.Portfolio.GetPlan, clientId, planId));
                planResponse.OnException(s => { throw new HttpClientException(s); });
                return planResponse.Resource;
            }
        }
    }
}