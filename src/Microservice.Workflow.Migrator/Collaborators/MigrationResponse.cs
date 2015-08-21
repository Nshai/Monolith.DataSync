namespace Microservice.Workflow.Migrator.Collaborators
{
    public class MigrationResponse : IRepresentation
    {
        public string Status { get; set; }
        public string Description { get; set; }
    }
}
