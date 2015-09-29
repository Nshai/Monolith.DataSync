using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Transactions;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1.Resources
{
    public class TemplateRoleResource : ITemplateRoleResource, IHandle<TemplateGroupUpdated>, IHandle<TemplateCloned>
    {
        private readonly ITemplateResource templateResource;
        private readonly IRepository<Template> templateRepository;

        public TemplateRoleResource(IRepository<Template> templateRepository, ITemplateResource templateResource)
        {
            this.templateRepository = templateRepository;
            this.templateResource = templateResource;
        }

        public TemplateRoleCollection ListRoles(int templateId)
        {
            var template = templateResource.GetTemplate(templateId);

            var roles = Mapper.Map<IEnumerable<TemplateRoleDocument>>(template.Roles).ToList();
            roles.ForEach(step => step.TemplateId = templateId);
            return new TemplateRoleCollection(roles)
            {
                TemplateId = templateId,
                TemplateVersionId = template.CurrentVersion.Id
            };
        }

        [Transaction]
        public TemplateRoleCollection PutRoles(int templateId, IEnumerable<int> roleIds)
        {
            var template = templateResource.GetTemplate(templateId);
            template.SetRoles(roleIds);
            templateRepository.Save(template);

            var roles = Mapper.Map<IEnumerable<TemplateRoleDocument>>(template.Roles).ToList();
            roles.ForEach(step => step.TemplateId = templateId);
            return new TemplateRoleCollection(roles)
            {
                TemplateId = templateId,
                TemplateVersionId = template.CurrentVersion.Id
            };
        }

        [Transaction]
        public void Handle(TemplateGroupUpdated args)
        {
            var template = templateResource.GetTemplate(args.TemplateId);
            
            template.CurrentVersion.Roles.Clear();

            templateRepository.Save(template);
        }

        [Transaction]
        public void Handle(TemplateCloned args)
        {
            var clonedFromTemplate = templateResource.GetTemplate(args.ClonedFromTemplateId);
            var template = templateResource.GetTemplate(args.TemplateId);

            template.SetRoles(clonedFromTemplate.Roles.Select(r => r.RoleId));

            templateRepository.Save(template);
        }
    }
}