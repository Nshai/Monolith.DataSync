using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using System.Threading;
using AutoMapper;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Principal;
using IntelliFlo.Platform.Transactions;
using Microservice.Workflow.Domain;
using Microservice.Workflow.Engine;
using Microservice.Workflow.v1.Activities;
using Microservice.Workflow.v1.Contracts;
using Newtonsoft.Json;
using NHibernate.Criterion;

namespace Microservice.Workflow.v1.Resources
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
        private readonly IReadOnlyRepository<InstanceStep> instanceStepRepository;
        private readonly ITrustedClientAuthenticationTokenBuilder tokenBuilder;

        public InstanceResource(IReadOnlyRepository<Instance> instanceRepository, IRepository<TemplateDefinition> templateDefinitionRepository, IRepository<InstanceHistory> instanceHistoryRepository, IWorkflowHost workflowHost, IReadOnlyRepository<InstanceStep> instanceStepRepository, ITrustedClientAuthenticationTokenBuilder tokenBuilder)
        {
            this.instanceRepository = instanceRepository;
            this.templateDefinitionRepository = templateDefinitionRepository;
            this.instanceHistoryRepository = instanceHistoryRepository;
            this.workflowHost = workflowHost;
            this.instanceStepRepository = instanceStepRepository;
            this.tokenBuilder = tokenBuilder;
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
            var tenantId = Thread.CurrentPrincipal.AsIFloPrincipal().TenantId;
            ICriterion[] additionalCriteria =
            {
                Restrictions.And(Restrictions.Eq("TenantId", tenantId), Restrictions.Eq("TemplateTenantId", tenantId))
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

        public InstanceDocument Restart(Guid instanceId, bool onlyAbortedInstances = true)
        {
            var instance = instanceRepository.Get(instanceId);
            if (instance == null)
                throw new InstanceNotFoundException();

            if (instance.TenantId != Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
                throw new InstancePermissionsException();

            if (onlyAbortedInstances)
            {
                if (instance.Status != InstanceStatus.Aborted.ToString())
                    throw new InstanceNotRestartableException();
            }

            var template = templateDefinitionRepository.Get(instance.Template.Id);

            var steps = GetInstanceSteps(instanceId).ToList();
            if (!steps.Any())
                throw new InstanceNotRestartableException();

            var currentStep = steps.Last();
            var stepId = currentStep.StepId;

            CreateTaskLog taskDetail = null;
            DelayLog delayDetail = null;
            foreach (var stepData in currentStep.Data)
            {
                if (currentStep.Step == StepName.CreateTask.ToString())
                {
                    taskDetail = stepData.Detail as CreateTaskLog;
                    if (taskDetail != null)
                        break;
                }
                else if (currentStep.Step == StepName.Delay.ToString())
                {
                    delayDetail = stepData.Detail as DelayLog;
                    if (delayDetail != null)
                        break;
                }
            }

            var delayedStart = (stepId == WorkflowConstants.DelayStartId);
            if (delayedStart)
                Check.IsTrue(delayDetail != null, "Delayed start time not found");
            
            string additionalContext = null;
            if (!delayedStart)
            {
                additionalContext = JsonConvert.SerializeObject(new AdditionalContext
                {
                    RunTo = new RunToAdditionalContext
                    {
                        StepId = stepId,
                        TaskId = taskDetail?.TaskId,
                        DelayTime = delayDetail?.DelayUntil
                    }
                });
            }

            var bearerToken = tokenBuilder.Build(DateTime.UtcNow, ClaimsPrincipal.Current);
            var newInstanceId = workflowHost.Create(template, new WorkflowContext
            {
                EntityType = instance.EntityType,
                EntityId = instance.EntityId,
                ClientId = instance.ParentEntityId,
                CorrelationId = instance.Id,
                RelatedEntityId = instance.RelatedEntityId,
                BearerToken = bearerToken,
                Start = delayedStart ? delayDetail.DelayUntil : DateTime.UtcNow,
                AdditionalContext = additionalContext,
                PreventDuplicates = false
            });
            
            return Get(newInstanceId);
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

        private IEnumerable<InstanceStep> GetInstanceSteps(Guid instanceId)
        {
            var systemSteps = new[] { StepName.Created.ToString(), StepName.Aborted.ToString(), StepName.Completed.ToString(), StepName.Errored.ToString(), StepName.Restarted.ToString() };
            return instanceStepRepository.Query().Where(i => i.InstanceId == instanceId && !systemSteps.Contains(i.Step)).OrderBy(i => i.StepIndex);
        }
    }
}