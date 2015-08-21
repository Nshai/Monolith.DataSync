using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Principal;
using IntelliFlo.Platform.Services.Workflow.Collaborators.v1;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.Engine;
using IntelliFlo.Platform.Services.Workflow.v1.Activities;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;
using IntelliFlo.Platform.Transactions;
using Newtonsoft.Json;
using NHibernate.Criterion;

namespace IntelliFlo.Platform.Services.Workflow.v1.Resources
{
    public class MigrationResource : IMigrationResource
    {
        private readonly IRepository<Template> templateRepository;
        private readonly IRepository<TemplateDefinition> templateDefinitionRepository;
        private readonly IServiceHttpClientFactory clientFactory;
        private readonly IReadOnlyRepository<Instance> instanceRepository;
        private readonly IReadOnlyRepository<InstanceStep> instanceStepRepository;
        private readonly IServiceAddressRegistry serviceAddressRegistry;
        private readonly ITrustedClientAuthenticationTokenBuilder tokenBuilder;
        private readonly IWorkflowHost workflowHost;
        private readonly IEventDispatcher eventDispatcher;

        public MigrationResource(IRepository<Template> templateRepository, IRepository<TemplateDefinition> templateDefinitionRepository, IReadOnlyRepository<Instance> instanceRepository, IReadOnlyRepository<InstanceStep> instanceStepRepository, IServiceAddressRegistry serviceAddressRegistry, IServiceHttpClientFactory clientFactory, ITrustedClientAuthenticationTokenBuilder tokenBuilder, IWorkflowHost workflowHost, IEventDispatcher eventDispatcher)
        {
            this.templateRepository = templateRepository;
            this.templateDefinitionRepository = templateDefinitionRepository;
            this.clientFactory = clientFactory;
            this.instanceRepository = instanceRepository;
            this.instanceStepRepository = instanceStepRepository;
            this.serviceAddressRegistry = serviceAddressRegistry;
            this.tokenBuilder = tokenBuilder;
            this.workflowHost = workflowHost;
            this.eventDispatcher = eventDispatcher;
        }

        public PagedResult<TemplateMigrationDocument> GetTemplates(string query, IDictionary<string, object> routeValues)
        {
            int count;
            var templates = templateRepository.ODataQueryWithInlineCount(query, out count);

            return new PagedResult<TemplateMigrationDocument>
            {
                Result = Mapper.Map<IEnumerable<TemplateMigrationDocument>>(templates),
                Count = count
            };
        }

        public PagedResult<InstanceDocument> GetInstances(string query, IDictionary<string, object> routeValues)
        {
            ICriterion[] additionalCriteria =
            {
                Restrictions.Or(
                    Restrictions.Eq("Status", "In Progress"),
                    Restrictions.Eq("Status", InstanceStatus.Processing.ToString()))
            };

            int count;
            var templates = instanceRepository.ODataQueryWithInlineCount(query, out count, additionalCriteria);

            return new PagedResult<InstanceDocument>
            {
                Result = Mapper.Map<IEnumerable<InstanceDocument>>(templates),
                Count = count
            };
        }

        [Transaction]
        public async Task<TemplateMigrationResponse> MigrateTemplate(int templateId)
        {
            var template = templateRepository.Get(templateId);
            if (template == null)
                throw new TemplateNotFoundException();

            // Don't migrate draft templates
            if (template.Status == WorkflowStatus.Draft)
                return new TemplateMigrationResponse() {Id = templateId, Status = MigrationStatus.Skipped.ToString()};

            var userSubject = await GetSubject(template.OwnerUserId);

            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(Principal.Constants.ApplicationClaimTypes.UserId, template.OwnerUserId.ToString(CultureInfo.InvariantCulture)));
            claimsIdentity.AddClaim(new Claim(Principal.Constants.ApplicationClaimTypes.TenantId, template.TenantId.ToString(CultureInfo.InvariantCulture)));
            claimsIdentity.AddClaim(new Claim(Principal.Constants.ApplicationClaimTypes.Subject, userSubject.ToString()));

            using (Thread.CurrentPrincipal.AsDelegate(() => new ClaimsPrincipal(claimsIdentity)))
            {
                eventDispatcher.Dispatch(new TemplateMadeInactive
                {
                    TemplateId = templateId
                });

                eventDispatcher.Dispatch(new TemplateMadeActive
                {
                    TemplateId = templateId
                });

                // If not active, then the net effect will be that the template definition is refreshed, but no triggers are left active
                if (template.Status != WorkflowStatus.Active)
                {
                    eventDispatcher.Dispatch(new TemplateMadeInactive
                    {
                        TemplateId = templateId
                    });
                }
            }

            return new TemplateMigrationResponse() {Id = templateId, Status = MigrationStatus.Success.ToString()};
        }

        public async Task<InstanceMigrationResponse> MigrateInstance(Guid instanceId)
        {
            var instance = instanceRepository.Get(instanceId);
            if (instance == null)
                throw new InstanceNotFoundException();

            if ((instance.Status != "In Progress" && instance.Status != InstanceStatus.Processing.ToString()) || instance.Template.Version >= TemplateDefinition.DefaultVersion)
                return new InstanceMigrationResponse() {Id = instanceId, Status = MigrationStatus.Skipped.ToString()};

            var userSubject = await GetSubject(instance.UserId);
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(Principal.Constants.ApplicationClaimTypes.UserId, instance.UserId.ToString(CultureInfo.InvariantCulture)),
                new Claim(Principal.Constants.ApplicationClaimTypes.TenantId, instance.TenantId.ToString(CultureInfo.InvariantCulture)),
                new Claim(Principal.Constants.ApplicationClaimTypes.Subject, userSubject.ToString())
            });
            
            var principal = new ClaimsPrincipal(identity);
            using (Thread.CurrentPrincipal.AsDelegate(() => principal))
            {
                var bearerToken = tokenBuilder.Build(DateTime.UtcNow, ClaimsPrincipal.Current);
                var uri = GetEndpointAddress(instance.Template.Id);

                // Resolve step index of current instance
                var steps = GetInstanceSteps(instanceId).ToList();
                var stepIndex = steps.Count() - 1;
                var currentStep = steps.ElementAt(stepIndex);

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

                var additionalContext = JsonConvert.SerializeObject(new AdditionalContext
                {
                    RunTo = new RunToAdditionalContext
                    {
                        StepIndex = stepIndex,
                        TaskId = taskDetail != null ? taskDetail.TaskId : (int?) null,
                        DelayTime = delayDetail != null ? delayDetail.DelayUntil : (DateTime?) null
                    }
                });

                var templateDefinition = templateDefinitionRepository.Get(instance.Template.Id);

                Guid? newInstanceId = workflowHost.Create(templateDefinition, new WorkflowContext
                {
                    EntityType = instance.EntityType,
                    EntityId = instance.EntityId,
                    ClientId = instance.ParentEntityId,
                    CorrelationId = instance.Id,
                    RelatedEntityId = instance.RelatedEntityId,
                    BearerToken = bearerToken,
                    Start = DateTime.UtcNow,
                    AdditionalContext = additionalContext,
                    PreventDuplicates = false
                });
                
                // TODO Merge instance history from old instance
                instanceRepository.ExecuteStoredProcedure<object>("workflow.dbo.SpNMigrationMergeInstances", new[] {new Parameter("InstanceId", instanceId), new Parameter("NewInstanceId", newInstanceId.Value)});

                return new InstanceMigrationResponse() {Id = instanceId, Status = MigrationStatus.Success.ToString()};
            }
        }

        private async Task<Guid> GetSubject(int userId)
        {
            using (var crmClient = clientFactory.Create("crm"))
            {
                var userInfoResponse = await crmClient.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoByUserId, userId));
                userInfoResponse.OnException(status => { throw new HttpResponseException(status); });

                var claims = userInfoResponse.Resource;

                Check.IsTrue(claims.ContainsKey(Principal.Constants.ApplicationClaimTypes.Subject), "Couldn't retrieve subject claim for user id {0}", userId);

                return Guid.Parse(claims[Principal.Constants.ApplicationClaimTypes.Subject].ToString());
            }
        }

        private IEnumerable<InstanceStep> GetInstanceSteps(Guid instanceId)
        {
            return instanceStepRepository.Query().Where(i => i.InstanceId == instanceId && i.Step != StepName.Created.ToString()).OrderBy(i => i.StepIndex);
        }

        private string GetEndpointAddress(Guid templateId, string relativeUri = null)
        {
            var workflowEndpointAddress = serviceAddressRegistry.GetServiceEndpoint("workflow").BaseAddress;
            var uri = new UriBuilder(workflowEndpointAddress);
            uri.Path += string.Format("{0}.xamlx", templateId);
            if (!string.IsNullOrEmpty(relativeUri))
            {
                uri.Path += string.Format("/{0}", relativeUri);
            }
            return uri.Uri.AbsoluteUri;
        }
    }
}