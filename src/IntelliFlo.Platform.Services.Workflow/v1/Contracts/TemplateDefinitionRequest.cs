using System;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
{
    public class TemplateDefinitionRequest
    {
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Definition { get; set; }
    }
}