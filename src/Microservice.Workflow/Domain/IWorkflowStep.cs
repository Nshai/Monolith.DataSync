using System;
using System.Activities;

namespace Microservice.Workflow.Domain
{
    public interface IWorkflowStep
    {
        Guid Id { get; }
        Activity GetActivity(IMapActivity mapActivity, Template template, int stepIndex);
        IWorkflowStep Patch(TemplateStepPatch request);
    }
}