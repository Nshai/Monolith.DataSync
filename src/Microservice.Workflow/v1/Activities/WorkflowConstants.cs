using System;

namespace Microservice.Workflow.v1.Activities
{
    public class WorkflowConstants
    {
        public const string WorkflowStepIdKey = "StepId";
        public const string WorkflowStepIndexKey = "StepIndex";
        public const string WorkflowStepNameKey = "Step";
        public const string WorkflowContextKey = "Ctx";
        public const string WorkflowBearerToken = "Bearer";

        public static Guid DelayStartId => new Guid("c68f0604-4504-450f-a50a-ef36d37f8bb1");
    }
}
