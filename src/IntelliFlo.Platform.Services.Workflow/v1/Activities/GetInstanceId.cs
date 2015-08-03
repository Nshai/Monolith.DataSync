using System;
using System.Activities;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public sealed class GetInstanceId : CodeActivity
    {
        public OutArgument<Guid> InstanceId { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            InstanceId.Set(context, context.WorkflowInstanceId);
        }
    }
}