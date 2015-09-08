using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microservice.Workflow.Migrator.Collaborators;

namespace Microservice.Workflow.Migrator.Impl
{
    public class InstanceMigrator : BaseMigrator<InstanceDocument>
    {
        private List<InstanceDocument> cachedInstances;
        private int totalCount = 0;
        private const int CachedBatchSize = 500;

        public InstanceMigrator(MigrateConfiguration configuration) : base("Instance", configuration) {}
        
        public override async Task<PagedCollection<InstanceDocument>> GetItems(Client client, int page)
        {
            // Since the migration effectively deletes and recreates the instances, rather than continually paging we get all instances at once
            // This avoid the chance of skipping instances if workflow is running slowly etc.
            if (cachedInstances == null)
            {
                cachedInstances = new List<InstanceDocument>();
                var cachedPage = page;
                do
                {
                    var uri = string.Format("v1/migrate/instances?$skip={0}&$top={1}", cachedPage * CachedBatchSize, CachedBatchSize);
                    if (configuration.InstanceId.HasValue)
                        uri += string.Format("&$filter=Id eq guid'{0}'", configuration.InstanceId);
                    else if(configuration.TenantId.HasValue)
                        uri += string.Format("&$filter=TenantId eq {0}", configuration.TenantId);

                    var instances = await client.Get<PagedCollection<InstanceDocument>>(uri);
                    totalCount = instances.Count;
                    cachedInstances.AddRange(instances.Items);
                    cachedPage++;
                } while (cachedInstances.Count < totalCount);
            }

            return new PagedCollection<InstanceDocument>()
            {
                Count = totalCount,
                Items = cachedInstances.OrderBy(i => i.Template.TemplateId).Skip(page * configuration.BatchSize).Take(configuration.BatchSize)
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
