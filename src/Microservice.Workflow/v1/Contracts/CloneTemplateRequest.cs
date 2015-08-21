using System.ComponentModel.DataAnnotations;

namespace Microservice.Workflow.v1.Contracts
{
    public class CloneTemplateRequest
    {
        [Required]
        public string Name { get; set; }
    }
}