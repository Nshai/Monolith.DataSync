using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Principal;
using IntelliFlo.Platform.Transactions;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;
using NHibernate.Criterion;

namespace Microservice.Workflow.v1.Resources
{
    public class TemplateCategoryResource : ITemplateCategoryResource
    {
        private readonly IRepository<TemplateCategory> categoryRepository;
        private readonly IRepository<Template> templateRepository;

        public TemplateCategoryResource(IRepository<TemplateCategory> categoryRepository, IRepository<Template> templateRepository)
        {
            this.categoryRepository = categoryRepository;
            this.templateRepository = templateRepository;
        }

        public TemplateCategoryDocument Get(int templateCategoryId)
        {
            var category = GetTemplateCategory(templateCategoryId);
            return Mapper.Map<TemplateCategoryDocument>(category);
        }

        public PagedResult<TemplateCategoryDocument> Query(string query, IDictionary<string, object> routeValues)
        {
            ICriterion[] additionalCriteria =
            {
                Restrictions.Eq("TenantId", Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
            };

            int count;
            var categories = categoryRepository.ODataQueryWithInlineCount(query, out count, additionalCriteria);

            return new PagedResult<TemplateCategoryDocument>
            {
                Result = Mapper.Map<IEnumerable<TemplateCategoryDocument>>(categories),
                Count = count
            };
        }

        [Transaction]
        public TemplateCategoryDocument Post(CreateTemplateCategoryRequest request)
        {
            Check.IsNotNull(request, "Request must be supplied");

            var tenantId = Thread.CurrentPrincipal.AsIFloPrincipal().TenantId;

            ValidateUniqueness(request.Name);
            var category = new TemplateCategory(request.Name, tenantId)
            {
                IsArchived = request.IsArchived
            };

            categoryRepository.Save(category);

            return Mapper.Map<TemplateCategoryDocument>(category);
        }

        [Transaction]
        public TemplateCategoryDocument Patch(int templateCategoryId, TemplateCategoryPatchRequest request)
        {
            Check.IsNotNull(request, "Request must be supplied");

            var category = GetTemplateCategory(templateCategoryId);

            if (!string.IsNullOrEmpty(request.Name))
            {
                ValidateUniqueness(request.Name, templateCategoryId);
                category.Name = request.Name;
            }

            category.IsArchived = request.IsArchived;

            categoryRepository.Save(category);

            return Mapper.Map<TemplateCategoryDocument>(category);
        }

        [Transaction]
        public void Delete(int templateCategoryId)
        {
            var category = GetTemplateCategory(templateCategoryId);

            var template = templateRepository.Query()
                .FirstOrDefault(x => x.TenantId == Thread.CurrentPrincipal.AsIFloPrincipal().TenantId && x.Category == category);

            if (template == null)
            {
                categoryRepository.Delete(category);
                return;
            }
            throw new TemplateCategoryInUseException(category.Name);

        }

        private void ValidateUniqueness(string name, int id = 0)
        {
            var existingCategories = categoryRepository.Query()
                .Where(
                    x =>
                    x.TenantId == Thread.CurrentPrincipal.AsIFloPrincipal().TenantId &&
                    x.Name.ToLowerInvariant() == name.ToLowerInvariant() &&
                    x.Id != id
                );

            if (!existingCategories.Any()) return;

            throw new TemplateCategoryNotUniqueException();
        }

        private TemplateCategory GetTemplateCategory(int templateCategoryId)
        {
            var template = categoryRepository.Get(templateCategoryId);
            if (template == null || template.TenantId != Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
                throw new TemplateCategoryNotFoundException();

            return template;
        }
    }
}