using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using IntelliFlo.Platform.Http.Client;
using log4net;
using Microservice.Workflow.Collaborators.v1;
using Microservice.Workflow.Domain;
using Constants = Microservice.Workflow.Engine.Constants;

namespace Microservice.Workflow.v1.Activities
{
    public sealed class CreateTask : NativeActivity, ILogActivity
    {
        private readonly ILog logger = LogManager.GetLogger(typeof (CreateTask));

        public InArgument<int> TaskTypeId { get; set; }
        public InArgument<int> DueDelay { get; set; }
        public InArgument<bool> DueDelayBusinessDays { get; set; }
        public InArgument<int> OwnerPartyId { get; set; }
        public InArgument<int> OwnerRoleId { get; set; }
        public InArgument<string> OwnerContextRole { get; set; }
        public InArgument<int> TemplateOwnerPartyId { get; set; }
        public OutArgument<int> TaskId { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var workflowContext = (WorkflowContext) context.Properties.Find(WorkflowConstants.WorkflowContextKey);

            using (UserContextBuilder.FromBearerToken(workflowContext.BearerToken))
            {
                var taskTypeId = TaskTypeId.Get(context);
                var dueDelay = DueDelay.Get(context);
                var dueDelayBusinessDays = DueDelayBusinessDays.Get(context);
                var ownerPartyId = OwnerPartyId.Get(context);
                var ownerRoleId = OwnerRoleId.Get(context);
                var ownerContextRole = OwnerContextRole.Get(context);
                var templateOwnerPartyId = TemplateOwnerPartyId.Get(context);

                var clientFactory = IoC.Resolve<IServiceHttpClientFactory>(Constants.ContainerId);
                using (var crmClient = clientFactory.Create("crm"))
                {
                    var dueDate = DateCalculator.AddDays(DateTime.UtcNow, TimeSpan.FromDays(dueDelay), dueDelayBusinessDays, (s, e) =>
                    {
                        HttpResponse<IEnumerable<HolidayDocument>> holidayResponse = null;
                        var holidayTask = crmClient.Get<IEnumerable<HolidayDocument>>(string.Format(Uris.Holidays.Get, s.ToString("s"), e.ToString("s")))
                            .ContinueWith(t =>
                            {
                                t.OnException(status => { throw new HttpClientException(status); });
                                holidayResponse = t.Result;
                            });

                        holidayTask.Wait();

                        return holidayResponse.Resource.Select(h => h.Date);
                    });

                    var taskBuilderFactory = IoC.Resolve<IEntityTaskBuilderFactory>(Constants.ContainerId);
                    var taskBuilder = taskBuilderFactory.Get(workflowContext.EntityType, this, context);
                    var taskResult = taskBuilder.Create(taskTypeId, dueDate, templateOwnerPartyId, ownerPartyId, ownerRoleId, ownerContextRole, workflowContext).ConfigureAwait(false);
                    var task = taskResult.GetAwaiter().GetResult();

                    logger.InfoFormat("Created task {0}", taskTypeId);
                    TaskId.Set(context, task.TaskId);
                }
            }
        }
    }
}