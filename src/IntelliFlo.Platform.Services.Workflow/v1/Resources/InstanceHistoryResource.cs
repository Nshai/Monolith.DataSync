﻿using System;
using System.Collections.Generic;
using AutoMapper;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;
using NHibernate.Criterion;

namespace IntelliFlo.Platform.Services.Workflow.v1.Resources
{
    public class InstanceHistoryResource : IInstanceHistoryResource
    {
        private readonly IRepository<InstanceHistory> instanceHistoryRepository;
        private readonly IRepository<InstanceStep> instanceStepRepository;

        public InstanceHistoryResource(IRepository<InstanceHistory> instanceHistoryRepository, IRepository<InstanceStep> instanceStepRepository)
        {
            this.instanceHistoryRepository = instanceHistoryRepository;
            this.instanceStepRepository = instanceStepRepository;
        }

        public PagedResult<InstanceHistoryDocument> QueryHistory(string query, IDictionary<string, object> routeValues)
        {
            var instanceId = routeValues["instanceId"];
            ICriterion[] additionalFilters =
            {
                Restrictions.Eq("InstanceId", new Guid(instanceId.ToString()))
            };

            int count;
            var stateHistory = instanceHistoryRepository.ODataQueryWithInlineCount(query, out count, additionalFilters);

            return new PagedResult<InstanceHistoryDocument>()
            {
                Result = Mapper.Map<IEnumerable<InstanceHistoryDocument>>(stateHistory),
                Count = count
            };
        }

        public PagedResult<InstanceStepDocument> QuerySteps(string query, IDictionary<string, object> routeValues)
        {
            var instanceId = routeValues["instanceId"];
            ICriterion[] additionalFilters =
            {
                Restrictions.Eq("InstanceId", new Guid(instanceId.ToString()))
            };

            int count;
            var stateHistory = instanceStepRepository.ODataQueryWithInlineCount(query, out count, additionalFilters);

            return new PagedResult<InstanceStepDocument>()
            {
                Result = Mapper.Map<IEnumerable<InstanceStepDocument>>(stateHistory),
                Count = count
            };
        }
    }
}