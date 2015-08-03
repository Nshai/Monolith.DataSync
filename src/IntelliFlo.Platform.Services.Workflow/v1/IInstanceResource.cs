using System;
using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;

namespace IntelliFlo.Platform.Services.Workflow.v1
{
    public interface IInstanceResource : IResource
    {
        InstanceDocument Get(Guid instanceId);
        PagedResult<InstanceDocument> Query(string query, IDictionary<string, object> routeValues);
        InstanceDocument Resume(Guid instanceId, string bookmarkName);
        InstanceDocument Abort(Guid instanceId);
        InstanceDocument Unsuspend(Guid instanceId);

    }
}