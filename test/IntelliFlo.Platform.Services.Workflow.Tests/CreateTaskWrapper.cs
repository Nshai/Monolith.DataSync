using System;
using System.Activities;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1;
using IntelliFlo.Platform.Services.Workflow.v1.Activities;
using CreateTaskStep = IntelliFlo.Platform.Services.Workflow.v1.Activities.CreateTaskStep;

namespace IntelliFlo.Platform.Services.Workflow.Tests
{
    public class CreateTaskWrapper : NativeActivity
    {
        public InArgument<WorkflowContext> Context { get; set; }
        private ActivityFunc<string, int, int, bool, int, int, string, int, Guid, int, int> Body { get; set; }

        public InArgument<string> TaskTransition { get; set; }
        public InArgument<Guid> StepId { get; set; }
        public InArgument<int> StepIndex { get; set; }
        public InArgument<int> TaskTypeId { get; set; }
        public InArgument<int> DueDelay { get; set; }
        public InArgument<bool> DueDelayBusinessDays { get; set; }
        public InArgument<int> OwnerPartyId { get; set; }
        public InArgument<int> OwnerRoleId { get; set; }
        public InArgument<string> OwnerContextRole { get; set; }
        public InArgument<int> TemplateOwnerPartyId { get; set; }
        public OutArgument<int> TaskId { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var taskTransition = new DelegateInArgument<string>();
            var taskTypeId = new DelegateInArgument<int>();
            var dueDelay = new DelegateInArgument<int>();
            var dueDelayBusinessDays = new DelegateInArgument<bool>();
            var ownerPartyId = new DelegateInArgument<int>();
            var ownerRoleId = new DelegateInArgument<int>();
            var ownerContextRole = new DelegateInArgument<string>();
            var templateOwnerPartyId = new DelegateInArgument<int>();
            var stepId = new DelegateInArgument<Guid>();
            var stepIndex = new DelegateInArgument<int>();

            Body = new ActivityFunc<string, int, int, bool, int, int, string, int, Guid, int, int>
            {
                Argument1 = taskTransition,
                Argument2 = taskTypeId,
                Argument3 = dueDelay,
                Argument4 = dueDelayBusinessDays,
                Argument5 = ownerPartyId,
                Argument6 = ownerRoleId,
                Argument7 = ownerContextRole,
                Argument8 = templateOwnerPartyId,
                Argument9 = stepId,
                Argument10 = stepIndex,
                Handler = new CreateTaskStep()
                {
                    TaskTransition = taskTransition,
                    TaskTypeId = taskTypeId,
                    DueDelay = dueDelay,
                    DueDelayBusinessDays = dueDelayBusinessDays,
                    OwnerPartyId = ownerPartyId,
                    OwnerRoleId = ownerRoleId,
                    OwnerContextRole = ownerContextRole,
                    TemplateOwnerPartyId = templateOwnerPartyId,
                    StepId = stepId,
                    StepIndex = stepIndex
                }
            };

            metadata.AddImplementationDelegate(Body);

            base.CacheMetadata(metadata);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var ctx = Context.Get(context);
            context.Properties.Add(WorkflowConstants.WorkflowContextKey, ctx);
            context.Properties.Add(WorkflowConstants.WorkflowStepNameKey, StepName.CreateTask);
            context.Properties.Add(WorkflowConstants.WorkflowStepIdKey, Guid.NewGuid());

            context.ScheduleFunc(Body, TaskTransition.Get(context), TaskTypeId.Get(context), DueDelay.Get(context), DueDelayBusinessDays.Get(context), OwnerPartyId.Get(context), OwnerRoleId.Get(context), OwnerContextRole.Get(context), TemplateOwnerPartyId.Get(context), StepId.Get(context), StepIndex.Get(context), ChildCompletionCallback);

        }

        private void ChildCompletionCallback(NativeActivityContext context, ActivityInstance completedInstance, int result)
        {
            TaskId.Set(context, result);
        }
    }
}
