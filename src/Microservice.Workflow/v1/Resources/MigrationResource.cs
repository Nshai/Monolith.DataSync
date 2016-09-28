using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Transactions;
using Microservice.Workflow.Collaborators.v1;
using Microservice.Workflow.Domain;
using Microservice.Workflow.Engine;
using Microservice.Workflow.v1.Activities;
using Microservice.Workflow.v1.Contracts;
using Newtonsoft.Json;
using NHibernate.Criterion;
using Check = IntelliFlo.Platform.Check;
using Instance = Microservice.Workflow.Domain.Instance;

namespace Microservice.Workflow.v1.Resources
{
    public class MigrationResource : IMigrationResource
    {
        private readonly IRepository<Template> templateRepository;
        private readonly IRepository<TemplateDefinition> templateDefinitionRepository;
        private readonly IHttpClientFactory clientFactory;
        private readonly IReadOnlyRepository<Instance> instanceRepository;
        private readonly IReadOnlyRepository<InstanceStep> instanceStepRepository;
        private readonly IServiceAddressRegistry serviceAddressRegistry;
        private readonly ITrustedClientAuthenticationTokenBuilder tokenBuilder;
        private readonly IWorkflowHost workflowHost;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IRepository<InstanceHistory> instanceHistoryRepository;
        private readonly IInstanceResource instanceResource;

        public MigrationResource(IRepository<Template> templateRepository, IRepository<TemplateDefinition> templateDefinitionRepository, IReadOnlyRepository<Instance> instanceRepository, IReadOnlyRepository<InstanceStep> instanceStepRepository, IServiceAddressRegistry serviceAddressRegistry, IHttpClientFactory clientFactory, ITrustedClientAuthenticationTokenBuilder tokenBuilder, IWorkflowHost workflowHost, IEventDispatcher eventDispatcher, IRepository<InstanceHistory> instanceHistoryRepository, IInstanceResource instanceResource)
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
            this.instanceHistoryRepository = instanceHistoryRepository;
            this.instanceResource = instanceResource;
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
                    Restrictions.Eq("Status", "Processing"))
            };

            int count;
            var templates = instanceRepository.ODataQueryWithInlineCount(query, out count, additionalCriteria);

            return new PagedResult<InstanceDocument>
            {
                Result = Mapper.Map<IEnumerable<InstanceDocument>>(templates),
                Count = count
            };
        }

        public PagedResult<InstanceStepDocument> GetInstanceSteps(string query, IDictionary<string, object> routeValues)
        {
            int count;
            var steps = instanceStepRepository.ODataQueryWithInlineCount(query, out count);

            return new PagedResult<InstanceStepDocument>
            {
                Result = Mapper.Map<IEnumerable<InstanceStepDocument>>(steps),
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
                return new TemplateMigrationResponse() {Id = templateId, Status = MigrationStatus.Skipped.ToString(), Description = "Draft templates aren't migrated"};


            var templateDefinition = templateDefinitionRepository.Get(template.Guid);
            if(templateDefinition.Version >= TemplateDefinition.DefaultVersion)
                return new TemplateMigrationResponse() { Id = templateId, Status = MigrationStatus.Skipped.ToString(), Description = "Template already migrated"};

            var userSubject = await GetSubject(template.OwnerUserId, template.TenantId);

            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.UserId, template.OwnerUserId.ToString(CultureInfo.InvariantCulture)));
            claimsIdentity.AddClaim(new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.TenantId, template.TenantId.ToString(CultureInfo.InvariantCulture)));
            claimsIdentity.AddClaim(new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.Subject, userSubject.ToString()));

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

        [Transaction]
        public async Task<InstanceMigrationResponse> MigrateInstance(Guid instanceId)
        {
            var instance = instanceRepository.Get(instanceId);
            if (instance == null)
                throw new InstanceNotFoundException();
            

            if ((instance.Status != "In Progress" && instance.Status != "Processing") || instance.Version >= TemplateDefinition.DefaultVersion)
                return new InstanceMigrationResponse() {Id = instanceId, Status = MigrationStatus.Skipped.ToString(), Description = "Instance already migrated"};

            var templateDefinition = templateDefinitionRepository.Get(instance.Template.Id);
            if(templateDefinition.Version < TemplateDefinition.DefaultVersion)
                return new InstanceMigrationResponse() { Id = instanceId, Status = MigrationStatus.Skipped.ToString(), Description = "Instance template was not migrated"};

            var template = templateRepository.Query().SingleOrDefault(t => t.Guid == instance.Template.Id);
            if (template == null)
                throw new TemplateNotFoundException();

            var userSubject = await GetSubject(instance.UserId, instance.TenantId);
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.UserId, instance.UserId.ToString(CultureInfo.InvariantCulture)),
                new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.TenantId, instance.TenantId.ToString(CultureInfo.InvariantCulture)),
                new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.Subject, userSubject.ToString())
            });
            
            var principal = new ClaimsPrincipal(identity);
            using (Thread.CurrentPrincipal.AsDelegate(() => principal))
            {
                var newInstance = instanceResource.Restart(instanceId, false);
                
                // TODO Merge instance history from old instance
                instanceRepository.ExecuteStoredProcedure<object>("workflow.dbo.SpNMigrationMergeInstances", new[] {new Parameter("InstanceId", instanceId), new Parameter("NewInstanceId", newInstance.Id)});

                return new InstanceMigrationResponse() {Id = instanceId, Status = MigrationStatus.Success.ToString()};
            }
        }
        
        private async Task<Guid> GetSubject(int userId, int tenantId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.UserId, userId.ToString(CultureInfo.InvariantCulture)),
                new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.TenantId, tenantId.ToString(CultureInfo.InvariantCulture)),
            });

            var principal = new ClaimsPrincipal(identity);
            using (Thread.CurrentPrincipal.AsDelegate(() => principal))
            using (var crmClient = clientFactory.Create("crm"))
            {
                var userInfoResponse = await crmClient.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoByUserId, userId));
                userInfoResponse.OnException(status => { throw new HttpResponseException(status); });

                var claims = userInfoResponse.Resource;

                Check.IsTrue(claims.ContainsKey(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.Subject), "Couldn't retrieve subject claim for user id {0}", userId);

                return Guid.Parse(claims[IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.Subject].ToString());
            }
        }
    }
}