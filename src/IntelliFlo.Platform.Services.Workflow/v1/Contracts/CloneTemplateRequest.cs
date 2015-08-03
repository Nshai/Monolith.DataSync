using System.ComponentModel.DataAnnotations;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
{
    public class CloneTemplateRequest
    {
        [Required]
        public string Name { get; set; }
    }
}