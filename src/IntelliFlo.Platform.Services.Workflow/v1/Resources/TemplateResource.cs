using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;
using IntelliFlo.Platform.Transactions;
using NHibernate.Criterion;

namespace IntelliFlo.Platform.Services.Workflow.v1.Resources
{
    /// <summary>
    /// Handle template requests
    /// </summary>
    public class TemplateResource : ITemplateResource, IHandle<TemplateMadeActive>
    {
        private const int SystemTenantId = 0;
        private readonly IServiceHttpClientFactory clientFactory;
        private readonly IRepository<TemplateCategory> templateCategoryRepository;
        private readonly IRepository<TemplateDefinition> templateDefinitionRepository;
        private readonly IRepository<Template> templateRepository;
        private readonly ITrustedClientAuthenticationTokenBuilder tokenBuilder;
        private readonly IWorkflowServiceFactory workflowServiceFactory;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IWorkflowHost workflowHost;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="templateRepository"></param>
        /// <param name="templateCategoryRepository"></param>
        /// <param name="tokenBuilder"></param>
        /// <param name="clientFactory"></param>
        /// <param name="templateDefinitionRepository"></param>
        /// <param name="workflowServiceFactory"></param>
        /// <param name="eventDispatcher"></param>
        /// <param name="workflowHost"></param>
        public TemplateResource(IRepository<Template> templateRepository,
            IRepository<TemplateCategory> templateCategoryRepository,
            ITrustedClientAuthenticationTokenBuilder tokenBuilder,
            IServiceHttpClientFactory clientFactory,
            IRepository<TemplateDefinition> templateDefinitionRepository, IWorkflowServiceFactory workflowServiceFactory, IEventDispatcher eventDispatcher, IWorkflowHost workflowHost)
        {
            this.templateRepository = templateRepository;
            this.templateCategoryRepository = templateCategoryRepository;
            this.tokenBuilder = tokenBuilder;
            this.clientFactory = clientFactory;
            this.templateDefinitionRepository = templateDefinitionRepository;
            this.workflowServiceFactory = workflowServiceFactory;
            this.eventDispatcher = eventDispatcher;
            this.workflowHost = workflowHost;
        }

        /// <summary>
        /// React to the TemplateMadeActive event
        /// </summary>
        /// <param name="args"></param>
        [Transaction]
        public void Handle(TemplateMadeActive args)
        {
            var template = GetTemplate(args.TemplateId);
            var templateGuid = template.Guid;

            var templateDefinition = templateDefinitionRepository.Get(templateGuid);
            var templateXaml = workflowServiceFactory.Build(template);

            if (templateDefinition != null)
            {
                templateDefinition.Name = template.Name;
                templateDefinition.Definition = templateXaml;
                templateDefinition.DateUtc = DateTime.UtcNow;
                templateDefinition.Version = TemplateDefinition.DefaultVersion;

                if (!templateDefinition.Compile())
                    throw new TemplateCompilationException();

                templateDefinitionRepository.Update(templateDefinition);
            }
            else
            {
                templateDefinition = new TemplateDefinition
                {
                    Id = templateGuid,
                    TenantId = template.TenantId,
                    Name = template.Name,
                    Definition = templateXaml,
                    DateUtc = DateTime.UtcNow,
                    Version = TemplateDefinition.DefaultVersion
                };

                if (!templateDefinition.Compile())
                    throw new TemplateCompilationException();

                templateDefinitionRepository.Save(templateDefinition);
            }
        }

        /// <summary>
        /// Gets a specified template
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public TemplateDocument Get(int templateId)
        {
            var template = GetTemplate(templateId);
            return Mapper.Map<TemplateDocument>(template);
        }

        /// <summary>
        /// Gets a specified template including steps
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public TemplateExtDocument GetExt(int templateId)
        {
            var template = GetTemplate(templateId);
            return Mapper.Map<TemplateExtDocument>(template);
        }

        /// <summary>
        /// List templates
        /// </summary>
        /// <param name="query"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public PagedResult<TemplateDocument> Query(string query, IDictionary<string, object> routeValues)
        {
            ICriterion[] additionalCriteria =
            {
                Restrictions.Eq("TenantId", (int) Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
            };

            int count;
            var templates = templateRepository.ODataQueryWithInlineCount(query, out count, additionalCriteria);

            return new PagedResult<TemplateDocument>
            {
                Result = Mapper.Map<IEnumerable<TemplateDocument>>(templates),
                Count = count
            };
        }

        public PagedResult<TemplateExtDocument> QueryExt(string query, IDictionary<string, object> routeValues)
        {
            ICriterion[] additionalCriteria =
            {
                Restrictions.Eq("TenantId", (int) Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
            };

            int count;
            var templates = templateRepository.ODataQueryWithInlineCount(query, out count, additionalCriteria);

            return new PagedResult<TemplateExtDocument>
            {
                Result = Mapper.Map<IEnumerable<TemplateExtDocument>>(templates),
                Count = count
            };
        }

        [Transaction]
        public TemplateDocument Post(CreateTemplateRequest request)
        {
            var template = CreateTemplate(request);
            return Mapper.Map<TemplateDocument>(template);
        }

        [Transaction]
        public TemplateDocument Clone(int templateId, CloneTemplateRequest request)
        {
            Check.IsNotNull(request, "Request must be supplied");
            var template = GetTemplate(templateId);

            var clonedTemplate = CreateTemplate(new CreateTemplateRequest
            {
                Name = request.Name,
                TemplateCategoryId = template.Category.Id,
                RelatedTo = template.RelatedTo.ToString(),
                OwnerUserId = template.OwnerUserId,
                ApplicableToGroupId = template.ApplicableToGroupId,
                IncludeSubGroups = template.IncludeSubGroups
            });

            clonedTemplate.Definition = template.Definition;

            eventDispatcher.Dispatch(new TemplateCloned
            {
                TemplateId = clonedTemplate.Id,
                ClonedFromTemplateId = templateId
            });

            return Mapper.Map<TemplateDocument>(clonedTemplate);
        }

        [Transaction]
        public TemplateDocument Patch(int templateId, TemplatePatchRequest request)
        {
            Check.IsNotNull(request, "Request must be supplied");

            var template = GetTemplate(templateId);

            if (!string.IsNullOrEmpty(request.Name))
            {
                ValidateUniqueness(request.Name, templateId);
                template.Name = request.Name;
            }

            if (request.OwnerUserId.HasValue)
                template.OwnerUserId = request.OwnerUserId.Value;

            if (request.TemplateCategoryId.HasValue)
                template.Category = templateCategoryRepository.Load(request.TemplateCategoryId);

            if (request.ApplicableToGroup.HasValue)
                template.ApplicableToGroupId = request.ApplicableToGroup.Value ? request.ApplicableToGroupId : null;

            if (request.IncludeSubGroups.HasValue)
                template.IncludeSubGroups = request.IncludeSubGroups;

            if (!string.IsNullOrEmpty(request.Status))
            {
                var status = (WorkflowStatus)Enum.Parse(typeof(WorkflowStatus), request.Status);
                template.SetStatus(status);
            }

            if (request.Notes != null)
                template.Notes = request.Notes;

            templateRepository.SaveWithDispatch(template, eventDispatcher);
            
            return Mapper.Map<TemplateDocument>(template);
        }

        [Transaction]
        public void Delete(int templateId)
        {
            var template = GetTemplate(templateId);
            template.MarkForDeletion();
            
            templateRepository.DeleteWithDispatch(template, eventDispatcher);
        }

        public void CreateInstance(int templateId, CreateInstanceRequest request, bool triggeredInstance = false)
        {
            Check.IsNotNull(request, "Request was not supplied");

            var template = GetTemplate(templateId);

            if(template.Status != WorkflowStatus.Active)
                throw new TemplateNotActiveException();

            var ids = GetRoleIdAndGroups();
            if (!template.IsUserPermittedToRun(ids.Item1, ids.Item2, triggeredInstance))
                throw new TemplatePermissionsException();

            var templateGuid = template.Guid;

            var bearerToken = tokenBuilder.Build(DateTime.UtcNow, Thread.CurrentPrincipal.AsIFloPrincipal());
            var templateDefinition = templateDefinitionRepository.Get(templateGuid);

            workflowHost.Create(templateDefinition, new WorkflowContext
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ClientId = request.ClientId,
                CorrelationId = request.CorrelationId,
                RelatedEntityId = request.RelatedEntityId,
                BearerToken = bearerToken,
                Start = request.Start ?? DateTime.UtcNow,
                AdditionalContext = request.AdditionalContext,
                PreventDuplicates = request.PreventDuplicates
            });
        }

        public void Initialise(int templateId)
        {
            var template = GetTemplate(templateId);

            if (template.Status != WorkflowStatus.Active)
                throw new TemplateNotActiveException();

            var templateDefinition = templateDefinitionRepository.Get(template.Guid);

            workflowHost.Initialise(templateDefinition);
        }

        public Template GetTemplate(int templateId)
        {
            var template = templateRepository.Get(templateId);
            if (template == null)
                throw new TemplateNotFoundException();

            if (template.TenantId != SystemTenantId && template.TenantId != Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
                throw new TemplatePermissionsException();

            return template;
        }

        private Template CreateTemplate(CreateTemplateRequest request)
        {
            Check.IsNotNull(request, "Request must be supplied");

            ValidateUniqueness(request.Name);

            var tenantId = Thread.CurrentPrincipal.AsIFloPrincipal().TenantId;
            var category = templateCategoryRepository.Load(request.TemplateCategoryId);
            var relatedTo = (WorkflowRelatedTo)Enum.Parse(typeof(WorkflowRelatedTo), request.RelatedTo);
            var ownerUserId = request.OwnerUserId;

            var template = new Template(request.Name, tenantId, category, relatedTo, ownerUserId)
            {
                ApplicableToGroupId = request.ApplicableToGroupId,
                IncludeSubGroups = request.IncludeSubGroups
            };

            templateRepository.SaveWithDispatch(template, eventDispatcher);

            return template;
        }

        private void ValidateUniqueness(string name, int id = 0)
        {
            var existingCategories = templateRepository.Query()
                .Where(
                    x =>
                        x.TenantId == Thread.CurrentPrincipal.AsIFloPrincipal().TenantId &&
                        x.Name.ToLowerInvariant() == name.ToLowerInvariant() &&
                        x.Id != id
                );

            if (!existingCategories.Any()) return;

            throw new TemplateNotUniqueException();
        }

        private Tuple<int, int[]> GetRoleIdAndGroups()
        {           
            string lineage;
            string roleId;

            var intellifloClaimsPrincipal = Thread.CurrentPrincipal.AsIFloPrincipal();

            if (!intellifloClaimsPrincipal.TryGetClaim(Principal.Constants.ApplicationClaimTypes.GroupLineage, out lineage) ||
                !intellifloClaimsPrincipal.TryGetClaim(Principal.Constants.ApplicationClaimTypes.RoleId, out roleId))
            {
                var subject = intellifloClaimsPrincipal.Subject;
                using (var crmClient = clientFactory.Create("crm"))
                {
                    HttpResponse<Dictionary<string, object>> userInfoResponse = null;
                    var userInfoTask = crmClient.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoBySubject, subject))
                        .ContinueWith(t =>
                        {
                            t.OnException(status => { throw new HttpResponseException(status); });
                            userInfoResponse = t.Result;
                        });

                    userInfoTask.Wait();

                    var claims = userInfoResponse.Resource;

                    lineage = claims.ContainsKey(Principal.Constants.ApplicationClaimTypes.GroupLineage) ? claims[Principal.Constants.ApplicationClaimTypes.GroupLineage].ToString() : string.Empty;

                    roleId = claims.ContainsKey(Principal.Constants.ApplicationClaimTypes.RoleId) ? claims[Principal.Constants.ApplicationClaimTypes.RoleId].ToString() : "0";
                }
            }

            var groups = GetGroupLineage(lineage);
            return Tuple.Create(int.Parse(roleId), groups);
        }

        private static int[] GetGroupLineage(string lineage)
        {
            return lineage.Split(',').Select(int.Parse).ToArray();
        }
    }
}