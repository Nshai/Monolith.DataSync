using System;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1;

namespace IntelliFlo.Platform.Services.Workflow.Engine
{
    public interface IWorkflowHost
    {
        Guid Create(TemplateDefinition template, WorkflowContext context);
        void CreateAsync(TemplateDefinition template, WorkflowContext context);
        void Resume(TemplateDefinition template, ResumeContext context);
        void Abort(TemplateDefinition template, Guid instanceId);
        void Unsuspend(TemplateDefinition template, Guid instanceId);
        void Initialise(TemplateDefinition template);
    }
}