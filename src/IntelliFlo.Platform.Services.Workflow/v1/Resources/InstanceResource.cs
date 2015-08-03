using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using AutoMapper;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Principal;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.Engine;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;
using IntelliFlo.Platform.Transactions;
using NHibernate.Criterion;

namespace IntelliFlo.Platform.Services.Workflow.v1.Resources
{
    /// <summary>
    /// </summary>
    /// <remarks>
    ///     Currently we don't do any permissions checking here as we have assumed that the caller has already checked this
    ///     Access to the API should be restricted to internal use only (ie calls from IO)
    /// </remarks>
    public class InstanceResource : IInstanceResource
    {
        private readonly IReadOnlyRepository<Instance> instanceRepository;
        private readonly IRepository<TemplateDefinition> templateDefinitionRepository;
        private readonly IRepository<InstanceHistory> instanceHistoryRepository;
        private readonly IWorkflowHost workflowHost;

        public InstanceResource(IReadOnlyRepository<Instance> instanceRepository, IRepository<TemplateDefinition> templateDefinitionRepository, IRepository<InstanceHistory> instanceHistoryRepository, IWorkflowHost workflowHost)
        {
            this.instanceRepository = instanceRepository;
            this.templateDefinitionRepository = templateDefinitionRepository;
            this.instanceHistoryRepository = instanceHistoryRepository;
            this.workflowHost = workflowHost;
        }

        public InstanceDocument Get(Guid instanceId)
        {
            var instance = instanceRepository.Get(instanceId);

            if (instance == null)
                throw new InstanceNotFoundException();

            if (instance.TenantId != Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
                throw new InstancePermissionsException();

            return Mapper.Map<InstanceDocument>(instance);
        }

        public PagedResult<InstanceDocument> Query(string query, IDictionary<string, object> routeValues)
        {
            ICriterion[] additionalCriteria =
            {
                Restrictions.Eq("TenantId", (int) Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
            };

            int count;
            var instances = instanceRepository.ODataQueryWithInlineCount(query, out count, additionalCriteria);

            return new PagedResult<InstanceDocument>
            {
                Result = Mapper.Map<IEnumerable<InstanceDocument>>(instances),
                Count = count
            };
        }

        public InstanceDocument Resume(Guid instanceId, string bookmarkName)
        {
            var instance = Get(instanceId);
            var template = templateDefinitionRepository.Get(instance.Template.TemplateId);

            workflowHost.Resume(template, new ResumeContext
            {
                InstanceId = instance.Id,
                BookmarkName = bookmarkName
            });
            return instance;
        }

        [Transaction]
        public InstanceDocument Abort(Guid instanceId)
        {
            var instance = Get(instanceId);

            var instanceHistory = InstanceHistory.Aborted(instanceId, SystemTime.Now());
            instanceHistory.IsComplete = false;
            instanceHistory.Data = new LogData { Detail = new AbortRequestLog { UserRequested = Thread.CurrentPrincipal.AsIFloPrincipal().UserId } };
            instanceHistoryRepository.Save(instanceHistory);

            var template = templateDefinitionRepository.Get(instance.Template.TemplateId);
            try
            {
                workflowHost.Abort(template, instanceId);
            }
            catch (FaultException ex)
            {
                if (ex.Code.Name == FaultCodes.InstanceNotFound)
                    throw new InstanceAbortException();
                throw;
            }

            return instance;
        }

        public InstanceDocument Unsuspend(Guid instanceId)
        {
            var instance = Get(instanceId);

            var template = templateDefinitionRepository.Get(instance.Template.TemplateId);
            try
            {
                workflowHost.Unsuspend(template, instanceId);
            }
            catch (FaultException ex)
            {
                if (ex.Code.Name == FaultCodes.InstanceNotFound)
                    throw new InstanceAbortException();
                throw;
            }

            return instance;
        }
    }
}