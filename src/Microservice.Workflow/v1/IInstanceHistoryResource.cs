﻿using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1
{
    public interface IInstanceHistoryResource : IResource
    {
        PagedResult<InstanceHistoryDocument> QueryHistory(string query, IDictionary<string, object> routeValues);
        PagedResult<InstanceStepDocument> QuerySteps(string query, IDictionary<string, object> routeValues);
    }
}