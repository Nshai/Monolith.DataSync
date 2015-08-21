using IntelliFlo.Platform.Http;

namespace Microservice.Workflow.v1.Contracts
{
    public class TemplateMigrationResponse : Representation
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        
        protected override void CreateHypermedia() {}
    }
}