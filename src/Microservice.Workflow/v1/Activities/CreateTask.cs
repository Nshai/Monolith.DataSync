using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Policy;
using Microservice.Workflow.Collaborators.v1;
using Microservice.Workflow.Domain;
using Autofac;
using IntelliFlo.Platform;
using Microservice.Workflow.Host;
using Microservice.Workflow.Utilities.TimeZone;

namespace Microservice.Workflow.v1.Activities
{
    public sealed class CreateTask : NativeActivity, ILogActivity
    {
        public InArgument<int> TaskTypeId { get; set; }
        public InArgument<int> DueDelay { get; set; }
        public InArgument<bool> DueDelayBusinessDays { get; set; }
        public InArgument<string> AssignedTo { get; set; }
        public InArgument<int> OwnerPartyId { get; set; }
        public InArgument<int> OwnerRoleId { get; set; }
        public InArgument<string> OwnerContextRole { get; set; }
        public InArgument<int> TemplateOwnerPartyId { get; set; }
        public OutArgument<int> TaskId { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var workflowContext = (WorkflowContext)context.Properties.Find(WorkflowConstants.WorkflowContextKey);

            using (var lifetimeScope = IoC.Container.BeginLifetimeScope(WorkflowScopes.Scope))
            using (var userPrincipal = UserContextBuilder.FromBearerToken(workflowContext.BearerToken, lifetimeScope))
            {
                var clientFactory = lifetimeScope.Resolve<IHttpClientFactory>();
                var timeZoneConverter = lifetimeScope.Resolve<ITimeZoneConverter>();
                var userId = userPrincipal.Value.UserId;
                var taskTypeId = TaskTypeId.Get(context);
                var dueDelay = DueDelay.Get(context);
                var dueDelayBusinessDays = DueDelayBusinessDays.Get(context);
                var ownerPartyId = OwnerPartyId.Get(context);
                var ownerRoleId = OwnerRoleId.Get(context);
                var ownerContextRole = OwnerContextRole.Get(context);
                var templateOwnerPartyId = TemplateOwnerPartyId.Get(context);
                var assignedTo = AssignedTo.Get(context);

                using (var crmClient = clientFactory.Create("crm"))
                {
                    var userTimeZone = GetUserTimeZone(crmClient, userId);

                    var userDateTimeNow = timeZoneConverter.ConvertFromUtc(SystemTime.Now(), userTimeZone);
                    var startDate = userDateTimeNow.Date;
                    var dueDate = DateCalculator.AddDays(userDateTimeNow, TimeSpan.FromDays(dueDelay), dueDelayBusinessDays, (s, e) =>
                    {
                        HttpResponse<IEnumerable<HolidayDocument>> holidayResponse = null;
                        var holidayTask = crmClient.UsingPolicy(HttpClientPolicy.Retry).SendAsync(c => c.Get<IEnumerable<HolidayDocument>>(string.Format(Uris.Holidays.Get, s.ToString("s"), e.ToString("s"))))
                            .ContinueWith(t =>
                            {
                                t.OnException(status => { throw new HttpClientException(status); });
                                holidayResponse = t.Result;
                            });

                        holidayTask.Wait();

                        return holidayResponse.Resource.Select(h => h.Date);
                    });

                    var taskBuilderFactory = lifetimeScope.Resolve<IEntityTaskBuilderFactory>();
                    var taskBuilder = taskBuilderFactory.Get(workflowContext.EntityType, this, context);
                    var taskResult = taskBuilder.Create(taskTypeId, startDate, dueDate, templateOwnerPartyId, assignedTo, ownerPartyId, ownerRoleId, ownerContextRole, workflowContext).ConfigureAwait(false);
                    var task = taskResult.GetAwaiter().GetResult();

                    TaskId.Set(context, task.TaskId);
                }
            }
        }

        private string GetUserTimeZone(IHttpClient crmClient, int userId)
        {
            string defaultTimeZone = "GMT";
            var userDocument = crmClient.UsingPolicy(HttpClientPolicy.Retry).
                SendAsync(c => c.Get<Collaborators.v2.UserDocument>(string.Format(Collaborators.v2.Uris.Crm.GetUserByUserId, userId.ToString())))
                .ConfigureAwait(false).GetAwaiter().GetResult();
            return userDocument.Resource.TimeZone ?? defaultTimeZone;
        }
    }
}
