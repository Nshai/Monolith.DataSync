using System.ComponentModel.DataAnnotations;

namespace Microservice.Workflow.v1.Contracts
{
    public class CreateTemplateCategoryRequest
    {
        [Required]
        public string Name { get; set; }
        public bool IsArchived { get; set; }
    }
}