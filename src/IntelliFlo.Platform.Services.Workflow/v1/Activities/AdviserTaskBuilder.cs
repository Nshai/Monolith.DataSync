using System.Activities;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Services.Workflow.Collaborators.v1;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public class AdviserTaskBuilder : EntityTaskBuilder
    {
        public AdviserTaskBuilder(IServiceHttpClientFactory clientFactory, Activity parentActivity, NativeActivityContext context) : base(clientFactory, parentActivity, context) { }

        public async override Task<int> GetContextPartyId(string ownerContextRole, WorkflowContext context)
        {
            using (var crmClient = ClientFactory.Create("crm"))
            {
                var adviserResponse = await crmClient.Get<AdviserDocument>(string.Format(Uris.Crm.GetAdviser, context.EntityId));
                adviserResponse.OnException(s => { throw new HttpClientException(s); });
                var adviser = adviserResponse.Resource;
                switch (ownerContextRole)
                {
                    case "TandCCoach":
                        return adviser.TnCCoachPartyId.HasValue ? adviser.TnCCoachPartyId.Value : 0;
                    case "Manager":
                        return adviser.ManagerId.HasValue ? adviser.ManagerId.Value : 0;
                    default:
                        return PartyNotFound;
                }
            }
            
        }

        public async override Task<int> PrepareRequest(CreateTaskRequest request, WorkflowContext context)
        {
            return context.EntityId;
        }
    }
}