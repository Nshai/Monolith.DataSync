using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1
{
    public interface ITemplateTriggerResource : IResource
    {
        TemplateTriggerCollection Get(int templateId);
        TemplateTriggerCollection Post(int templateId, CreateTemplateTrigger request);
    }
}