using System;
using System.Activities;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Policy;
using V1 = Microservice.Workflow.Collaborators.v1;
using V2 = Microservice.Workflow.Collaborators.v2;
using Microservice.Workflow.Domain;
using Constants = IntelliFlo.Platform.Principal.Constants;

namespace Microservice.Workflow.v1.Activities
{
    public class ServiceCaseTaskBuilder : EntityTaskBuilder
    {
        public ServiceCaseTaskBuilder(IHttpClientFactory clientFactory, Activity parentActivity, NativeActivityContext context) : base(clientFactory, parentActivity, context) { }

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
                    case RoleContextType.Adviser:
                        {
                            var serviceCaseDocumentV1 = await GetServiceCase(crmClient, context.ClientId, context.EntityId);

                            return serviceCaseDocumentV1.ServicingAdviserId == 0 
                                ? PartyNotFound 
                                : serviceCaseDocumentV1.ServicingAdviserId;
                        }
                    case RoleContextType.ServicingAdministrator:
                    case RoleContextType.Paraplanner:
                        {
                            var serviceCaseResponseV2 = await crmClient.UsingPolicy(HttpClientPolicy.Retry)
                                                                     .SendAsync(c => c.Get<V2.ServiceCaseDocument>(string.Format(V2.Uris.Crm.GetServiceCase, context.ClientId, context.EntityId)))
                                                                     .OnException(s => { throw new HttpClientException(s); });

                            var serviceCaseDocumentV2 = serviceCaseResponseV2.Resource;

                            var entityId = GetEntityId(contextRole, serviceCaseDocumentV2);

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
            V1.ServiceCaseDocument serviceCase = null;

            using (var crmClient = ClientFactory.Create("crm"))
            {
                serviceCase = await GetServiceCase(crmClient, context.ClientId, context.EntityId);
            }

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

        private static async Task<V1.ServiceCaseDocument> GetServiceCase(IHttpClient crmHttpClient, int clientId, int serviceCaseId)
        {
            var serviceCaseResponse = await crmHttpClient.UsingPolicy(HttpClientPolicy.Retry)
                                                         .SendAsync(c => c.Get<V1.ServiceCaseDocument>(string.Format(V1.Uris.Crm.GetServiceCase, clientId, serviceCaseId)))
                                                         .OnException(s => { throw new HttpClientException(s); });

            return serviceCaseResponse.Resource;
        }

        private static int? GetEntityId(RoleContextType contextRole, V2.ServiceCaseDocument serviceCase)
        {
            return contextRole == RoleContextType.ServicingAdministrator
                ? serviceCase.ServicingAdministrator?.Id
                : serviceCase.Paraplanner?.Id;
        }
    }
}