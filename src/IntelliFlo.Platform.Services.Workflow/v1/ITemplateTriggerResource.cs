using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;

namespace IntelliFlo.Platform.Services.Workflow.v1
{
    public interface ITemplateTriggerResource : IResource
    {
        TemplateTriggerCollection Get(int templateId);
        TemplateTriggerCollection Post(int templateId, CreateTemplateTrigger request);
    }
}