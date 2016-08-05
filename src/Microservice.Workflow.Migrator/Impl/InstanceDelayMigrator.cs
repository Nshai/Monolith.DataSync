using System.Threading.Tasks;
using Microservice.Workflow.Migrator.Collaborators;

namespace Microservice.Workflow.Migrator.Impl
{
    public class InstanceDelayMigrator : BaseMigrator<InstanceStepDocument>
    {
        public InstanceDelayMigrator(MigrateConfiguration configuration) : base("InstanceDelay", configuration) {}
        public override async Task<PagedCollection<InstanceStepDocument>> GetItems(Client client, int page)
        {
            var filter = "Step eq 'Delay' and IsComplete eq false";
            if (configuration.InstanceId.HasValue)
                filter += $" and InstanceId eq guid'{configuration.InstanceId}'";
            var uri = $"v1/migrate/instancesteps?$filter={filter}&$skip={page * configuration.BatchSize}&$top={configuration.BatchSize}";

            return await client.Get<PagedCollection<InstanceStepDocument>>(uri);
        }

        public override async Task<MigrationResponse> MigrateItem(Client client, string id)
        {
            var uri = $"v1/migrate/instances/{id}";
            return await client.Post<MigrationResponse>(uri);
        }

        public override string GetId(InstanceStepDocument item)
        {
            return item.InstanceId.ToString();
        }
    }
}
