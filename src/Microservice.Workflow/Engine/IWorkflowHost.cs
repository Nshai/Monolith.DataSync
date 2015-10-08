using System;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1;

namespace Microservice.Workflow.Engine
{
    public interface IWorkflowHost : IDisposable
    {
        Guid Create(TemplateDefinition template, WorkflowContext context);
        void CreateAsync(TemplateDefinition template, WorkflowContext context);
        void Resume(TemplateDefinition template, ResumeContext context);
        void Abort(TemplateDefinition template, Guid instanceId);
        void Unsuspend(TemplateDefinition template, Guid instanceId);
        void Initialise(TemplateDefinition template);
        void IncrementInstanceCount(Guid templateId, Guid instanceId);
        void DecrementInstanceCount(Guid templateId, Guid instanceId);
    }
}