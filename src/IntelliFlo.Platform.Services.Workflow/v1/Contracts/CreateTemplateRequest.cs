using System.ComponentModel.DataAnnotations;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.Domain;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
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
