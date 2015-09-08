using System;

namespace Microservice.Workflow.Migrator
{
    public class MigrateConfiguration
    {
        public MigrationType Type { get; set; }
        public bool MigrateTemplates
        {
            get { return Type == MigrationType.Both || Type == MigrationType.Template; }
        }

        public bool MigrateInstances
        {
            get { return Type == MigrationType.Both || Type == MigrationType.Instance; }
        }

        public Guid? InstanceId { get; set; }
        public int? TemplateId { get; set; }
        public int BatchSize { get { return 100; } }
        public int? TenantId { get; set; }
    }
}