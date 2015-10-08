namespace Microservice.Workflow.SubSystemTests.v1.Models
{
    public class CreateTemplateRequest
    {
        public string Name { get; set; }
        public string RelatedTo { get; set; }
        public int TemplateCategoryId { get; set; }
        public int OwnerUserId { get; set; }
        public int? ApplicableToGroupId { get; set; }
        public bool? IncludeSubGroups { get; set; }
    }
}