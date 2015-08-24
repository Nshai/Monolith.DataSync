using System;
using System.Activities;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Principal;
using Microservice.Workflow.Collaborators.v1;
using Microservice.Workflow.Domain;
using Check = IntelliFlo.Platform.Check;
using Constants = IntelliFlo.Platform.Principal.Constants;

namespace Microservice.Workflow.v1.Activities
{
    public class EntityTaskBuilder
    {
        private readonly IServiceHttpClientFactory clientFactory;
        private readonly Activity parentActivity;
        private readonly NativeActivityContext context;
        public const int PartyNotFound = 0;

        public EntityTaskBuilder(IServiceHttpClientFactory clientFactory, Activity parentActivity, NativeActivityContext context)
        {
            this.clientFactory = clientFactory;
            this.parentActivity = parentActivity;
            this.context = context;
        }

        public async Task<TaskDocument> Create(int taskTypeId, DateTime dueDate, int templateOwnerPartyId, string assignedTo, int ownerPartyId, int ownerRoleId, string ownerContextRole, WorkflowContext workflowContext)
        {
            var taskRequest = new CreateTaskRequest
            {
                TaskTypeId = taskTypeId,
                DueDate = dueDate,
                AssignedByPartyId = templateOwnerPartyId
            };

            if (assignedTo == "ContextRole")
            {
                var contextPartyId = await GetContextPartyId(ownerContextRole, workflowContext);
                taskRequest.AssignedToPartyId = contextPartyId != PartyNotFound ? contextPartyId : templateOwnerPartyId;
            }
            else if (assignedTo == "Role")
            {
                // TODO Replace fallback when role doesn't exist?
                taskRequest.AssignedToRoleId = ownerRoleId;
            }
            else if (assignedTo == "LoggedInUser")
            {
                taskRequest.AssignedToPartyId = await GetUserPartyId();
            }
            else
            {
                taskRequest.AssignedToPartyId = ownerPartyId;
            }

            if ((!taskRequest.AssignedToPartyId.HasValue || taskRequest.AssignedToPartyId == 0) && (!taskRequest.AssignedToRoleId.HasValue || taskRequest.AssignedToRoleId == 0))
                throw new FaultException("Failed to create task because assigned user or role could not be resolved", new FaultCode(FaultCodes.CreateTaskFailed));
            
            await PrepareRequest(taskRequest, workflowContext);

            try
            {
                var task = await CreateTask(taskRequest);

                parentActivity.LogMessage(context, false, new LogData
                {
                    Detail = new CreateTaskLog
                    {
                        TaskId = task.TaskId,
                        AssignedUserId = Thread.CurrentPrincipal.AsIFloPrincipal().UserId,
                        AssignedRoleId = task.AssignedToRoleId,
                        AssignedPartyId = task.AssignedToPartyId
                    }
                });

                return task;
            }
            catch (HttpClientException ex)
            {
                throw new FaultException(string.Format("Failed to create task : {0}", ex.Message), new FaultCode(FaultCodes.CreateTaskFailed));
            }
        }
        
        public async virtual Task<int> PrepareRequest(CreateTaskRequest request, WorkflowContext context)
        {
            return context.EntityId;
        }

        public async virtual Task<int> GetContextPartyId(string ownerContextRole, WorkflowContext context)
        {
            return PartyNotFound;
        }

        protected IServiceHttpClientFactory ClientFactory
        {
            get { return clientFactory; }
        }

        private async Task<TaskDocument> CreateTask(CreateTaskRequest request)
        {
            using (var crmClient = ClientFactory.Create("crm"))
            {
                var taskResponse = await crmClient.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, request);
                taskResponse.OnException(s => { throw new HttpClientException(s); });
                return taskResponse.Resource;
            }
        }

        private async Task<int> GetUserPartyId()
        {
            using (var crmClient = clientFactory.Create("crm"))
            {
                var subject = Thread.CurrentPrincipal.AsIFloPrincipal().Subject;
                HttpResponse<Dictionary<string, object>> userInfoResponse = null;
                var userInfoTask = crmClient.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoBySubject, subject))
                    .ContinueWith(t =>
                    {
                        t.OnException(status => { throw new HttpClientException(status); });
                        userInfoResponse = t.Result;
                    });

                userInfoTask.Wait();

                var claims = userInfoResponse.Resource;

                Check.IsTrue(claims.ContainsKey(Constants.ApplicationClaimTypes.PartyId), "Couldn't retrieve party id claim for user subject {0}", subject);

                return int.Parse(claims[Constants.ApplicationClaimTypes.PartyId].ToString());
            }
        }
    }
}