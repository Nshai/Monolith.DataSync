using System;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1
{
    public interface ITemplateStepResource : IResource
    {
        TemplateStepDocument Post(int templateId, CreateTemplateStepRequest request);
        TemplateStepDocument Get(int templateId, Guid stepId);
        TemplateStepDocument MoveStepUp(int templateId, Guid stepId);
        TemplateStepDocument MoveStepDown(int templateId, Guid stepId);
        void Delete(int templateId, Guid stepId);
        TemplateStepCollection List(int templateId);
        TemplateStepDocument Patch(int templateId, Guid stepId, TemplateStepPatchRequest request);
    }
}