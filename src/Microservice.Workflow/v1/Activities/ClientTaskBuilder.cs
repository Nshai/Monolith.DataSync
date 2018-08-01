using System;
using System.Activities;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Policy;
using V1 = Microservice.Workflow.Collaborators.v1;
using V2 = Microservice.Workflow.Collaborators.v2;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Activities
{
    public class ClientTaskBuilder : EntityTaskBuilder
    {
        public ClientTaskBuilder(IHttpClientFactory clientFactory, Activity parentActivity, NativeActivityContext context) : base(clientFactory, parentActivity, context) { }

        public async override Task<int> GetContextPartyId(string ownerContextRole, WorkflowContext context)
        {
            RoleContextType contextRole;

            if (!Enum.TryParse(ownerContextRole, out contextRole))
            {
                return PartyNotFound;
            }

            using (var crmClient = ClientFactory.Create("crm"))
            {
                switch (contextRole)
                {
                    case RoleContextType.ServicingAdviser:
                    {
                        var clientResponseV1 = await crmClient.UsingPolicy(HttpClientPolicy.Retry)
                                                            .SendAsync(c => c.Get<V1.ClientDocument>(string.Format(V1.Uris.Crm.GetClient, context.EntityId)))
                                                            .OnException(s => { throw new HttpClientException(s); });

                        var clientDocumentV1 = clientResponseV1.Resource;
                        return clientDocumentV1.CurrentAdviserPartyId;
                    }
                    case RoleContextType.ServicingAdministrator:
                    case RoleContextType.Paraplanner:
                    {
                        var clientResponseV2 = await crmClient.UsingPolicy(HttpClientPolicy.Retry)
                                                              .SendAsync(c => c.Get<V2.ClientDocument>(string.Format(V2.Uris.Crm.GetClient, context.EntityId)))
                                                              .OnException(s => { throw new HttpClientException(s); });

                        var clientDocumentV2 = clientResponseV2.Resource;

                        var entityId = GetEntityId(contextRole, clientDocumentV2);

                        if (!entityId.HasValue)
                        {
                            return PartyNotFound;
                        }

                        var userResponse = await crmClient.UsingPolicy(HttpClientPolicy.Retry)
                                                                .SendAsync(c => c.Get<V1.UserDocument>(string.Format(V1.Uris.Crm.GetUserByUserId, entityId)))
                                                                .OnException(s => { throw new HttpClientException(s); });

                        var user = userResponse.Resource;

                        return user.PartyId;
                    }
                    default:
                        return PartyNotFound;
                }
            }
        }

        public async override Task<int> PrepareRequest(V1.CreateTaskRequest request, WorkflowContext context)
        {
            request.RelatedPartyId = context.EntityId;
            if (context.RelatedEntityId > 0)
                request.RelatedJointPartyId = context.RelatedEntityId;
            return context.EntityId;
        }

        private int? GetEntityId(RoleContextType contextRole, V2.ClientDocument client)
        {
            return contextRole == RoleContextType.ServicingAdministrator
                ? client.ServicingAdministrator?.Id
                : client.Paraplanner?.Id;

        }
    }
}