namespace Microservice.Workflow.v1.Contracts
{
    public class TemplateCategoryPatchRequest
    {
        public string Name { get; set; }
        public bool IsArchived { get; set; }
    }
}