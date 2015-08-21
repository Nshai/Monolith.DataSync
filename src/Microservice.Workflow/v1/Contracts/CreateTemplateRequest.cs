using System.ComponentModel.DataAnnotations;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class CreateTemplateRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [ValidEnumValues(typeof(WorkflowRelatedTo))]
        public string RelatedTo { get; set; }
        
        [Required]
        public int TemplateCategoryId { get; set; }
        
        [Required]
        public int OwnerUserId { get; set; }
        
        public int? ApplicableToGroupId { get; set; }
        public bool? IncludeSubGroups { get; set; }
    }
}
