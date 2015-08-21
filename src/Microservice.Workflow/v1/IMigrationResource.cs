using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1
{
    public interface IMigrationResource : IResource
    {
        PagedResult<TemplateMigrationDocument> GetTemplates(string query, IDictionary<string, object> routeValues);
        Task<TemplateMigrationResponse> MigrateTemplate(int templateId);
        Task<InstanceMigrationResponse> MigrateInstance(Guid instanceId);
        PagedResult<InstanceDocument> GetInstances(string query, IDictionary<string, object> routeValues);
    }
}