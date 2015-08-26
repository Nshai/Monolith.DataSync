using System;
using IntelliFlo.Platform;

namespace Microservice.Workflow.Domain
{
    public static class TemplateStepFactory
    {
        public static IWorkflowStep Create(CreateTemplateStep request)
        {
            var stepType = (StepType)Enum.Parse(typeof(StepType), request.Type);
            switch (stepType)
            {
                case StepType.CreateTask:
                    return new CreateTaskStep(
                        request.StepId ?? GuidCombGenerator.Generate(),
                        (TaskTransition)Enum.Parse(typeof(TaskTransition), request.Transition),
                        request.TaskTypeId.Value,
                        (TaskAssignee)Enum.Parse(typeof(TaskAssignee), request.AssignedTo),
                        request.Delay ?? 0,
                        request.DelayBusinessDays ?? false,
                        request.AssignedToPartyId,
                        request.AssignedToRoleId,
                        !string.IsNullOrEmpty(request.AssignedToRoleContext) ? (RoleContextType)Enum.Parse(typeof(RoleContextType), request.AssignedToRoleContext) : (RoleContextType?) null);

                case StepType.Delay:
                    return new DelayStep(
                        request.StepId ?? GuidCombGenerator.Generate(),
                        request.Delay.Value,
                        request.DelayBusinessDays ?? false);
            }

            throw new ValidationException("Request was invalid");
        }
    }
}
