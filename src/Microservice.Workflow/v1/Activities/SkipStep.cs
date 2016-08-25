using System;
using System.Activities;
using Newtonsoft.Json;

namespace Microservice.Workflow.v1.Activities
{
    public sealed class SkipStep : NativeActivity
    {
        public OutArgument<SkipState> Skip { get; set; }
        public OutArgument<int?> TaskId { get; set; }
        public OutArgument<DateTime?> DelayTime { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var workflowContext = (WorkflowContext)context.Properties.Find(WorkflowConstants.WorkflowContextKey);
            var currentStepId = (Guid)context.Properties.Find(WorkflowConstants.WorkflowStepIdKey);
            var currentStepIndex = (int)context.Properties.Find(WorkflowConstants.WorkflowStepIndexKey);

            if (string.IsNullOrEmpty(workflowContext.AdditionalContext))
            {
                Skip.Set(context, SkipState.Continue);
                return;
            }

            var additionalContext = JsonConvert.DeserializeObject<AdditionalContext>(workflowContext.AdditionalContext);
            var runToContext = additionalContext.RunTo;
            if (runToContext == null || runToContext.StepIndex < 0)
            {
                Skip.Set(context, SkipState.Continue);
                MarkInstanceAsResumed(workflowContext);
                return;
            }

            if (runToContext.StepId.HasValue)
            {
                if (currentStepId == runToContext.StepId)
                {
                    Skip.Set(context, SkipState.TargetStep);
                    TaskId.Set(context, runToContext.TaskId);
                    DelayTime.Set(context, runToContext.DelayTime);
                    MarkInstanceAsResumed(workflowContext);
                }
                else
                {
                    Skip.Set(context, SkipState.Skip);
                }
            }
            else
            {
                if (currentStepIndex < runToContext.StepIndex)
                {
                    Skip.Set(context, SkipState.Skip);
                    return;
                }
                if (currentStepIndex == runToContext.StepIndex)
                {
                    Skip.Set(context, SkipState.TargetStep);
                    TaskId.Set(context, runToContext.TaskId);
                    DelayTime.Set(context, runToContext.DelayTime);
                }
                else if (currentStepIndex > runToContext.StepIndex)
                {
                    Skip.Set(context, SkipState.Continue);
                }

                MarkInstanceAsResumed(workflowContext);
            }
        }

        private void MarkInstanceAsResumed(WorkflowContext ctx)
        {
            ctx.AdditionalContext = null;
        }
    }
}
