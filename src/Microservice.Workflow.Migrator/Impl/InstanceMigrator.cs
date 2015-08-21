using System.Linq;
using System.Threading.Tasks;
using Microservice.Workflow.Migrator.Collaborators;

namespace Microservice.Workflow.Migrator.Impl
{
    public class InstanceMigrator : BaseMigrator<InstanceDocument>
    {
        private PagedCollection<InstanceDocument> instances;

        public InstanceMigrator(MigrateConfiguration configuration) : base("Instance", configuration) {}
        
        public override async Task<PagedCollection<InstanceDocument>> GetItems(Client client, int page)
        {
            // Since the migration effectively deletes and recreates the instances, rather than continually paging we get all instances at once
            // This avoid the chance of skipping instances if workflow is running slowly etc.
            if (instances == null)
            {
                var uri = string.Format("v1/migrate/instances");
                if (configuration.InstanceId.HasValue)
                    uri += string.Format("?$filter=Id eq guid'{0}'", configuration.InstanceId);
                instances = await client.Get<PagedCollection<InstanceDocument>>(uri);
            }

            return new PagedCollection<InstanceDocument>()
            {
                Count = instances.Count,
                Items = instances.Items.Skip(page * configuration.BatchSize).Take(configuration.BatchSize)
            };
        }

        public override async Task<MigrationResponse> MigrateItem(Client client, string id)
        {
            var uri = string.Format("v1/migrate/instances/{0}", id);
            return await client.Post<MigrationResponse>(uri);
        }

        public override string GetId(InstanceDocument item)
        {
            return item.Id.ToString();
        }
    }
}
