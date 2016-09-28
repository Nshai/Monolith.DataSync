using System;
using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1
{
    public interface IInstanceResource : IResource
    {
        InstanceDocument Get(Guid instanceId);
        PagedResult<InstanceDocument> Query(string query, IDictionary<string, object> routeValues);
        InstanceDocument Resume(Guid instanceId, string bookmarkName);
        InstanceDocument Abort(Guid instanceId);
        InstanceDocument Unsuspend(Guid instanceId);

        InstanceDocument Restart(Guid instanceId, bool onlyAbortedInstances = true);
    }
}