using System;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Documentation.Annotations;

namespace Microservice.Workflow.v1.Contracts
{
    [SwaggerDefinition("InstanceMigration")]
    public class InstanceMigrationResponse : Representation
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        protected override void CreateHypermedia() { }
    }
}