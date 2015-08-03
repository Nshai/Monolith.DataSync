using System;
using IntelliFlo.Platform.Http;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
{
    public class InstanceMigrationResponse : Representation
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        protected override void CreateHypermedia() { }
    }
}