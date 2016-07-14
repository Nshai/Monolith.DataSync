using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Documentation.Annotations;

namespace Microservice.Workflow.v1.Contracts
{
    [SwaggerDefinition("TemplateMigration")]
    public class TemplateMigrationResponse : Representation
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        
        protected override void CreateHypermedia() {}
    }
}