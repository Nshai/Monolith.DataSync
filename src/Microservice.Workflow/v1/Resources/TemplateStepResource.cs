using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using IntelliFlo.Platform;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Transactions;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1.Resources
{
    public class TemplateStepResource : ITemplateStepResource
    {
        private readonly ITemplateResource templateResource;
        private readonly IRepository<Template> templateRepository;

        public TemplateStepResource(IRepository<Template> templateRepository, ITemplateResource templateResource)
        {
            this.templateRepository = templateRepository;
            this.templateResource = templateResource;
        }

        [Transaction]
        public TemplateStepDocument Post(int templateId, CreateTemplateStepRequest request)
        {
            Check.IsNotNull(request, "Request must be supplied");

            var createStep = Mapper.Map<CreateTemplateStep>(request);

            var template = templateResource.GetTemplate(templateId);
            var step = TemplateStepFactory.Create(createStep);
            template.AddStep(step);
            templateRepository.Save(template);
            
            var stepDocument = Mapper.Map<TemplateStepDocument>(step);
            stepDocument.TemplateId = templateId;
            return stepDocument;
        }

        public TemplateStepDocument Get(int templateId, Guid stepId)
        {
            var template = templateResource.GetTemplate(templateId);
            var step = template.Steps.SingleOrDefault(s => s.Id == stepId);
            if (step == null)
                throw new TemplateStepNotFoundException();

            var stepDocument = Mapper.Map<TemplateStepDocument>(step);
            stepDocument.TemplateId = templateId;
            return stepDocument;
        }

        [Transaction]
        public TemplateStepDocument MoveStepUp(int templateId, Guid stepId)
        {
            var template = templateResource.GetTemplate(templateId);
            var step = template.MoveStepUp(stepId);

            templateRepository.Save(template);

            var stepDocument = Mapper.Map<TemplateStepDocument>(step);
            stepDocument.TemplateId = templateId;
            return stepDocument;
        }

        [Transaction]
        public TemplateStepDocument MoveStepDown(int templateId, Guid stepId)
        {
            var template = templateResource.GetTemplate(templateId);
            var step = template.MoveStepDown(stepId);

            templateRepository.Save(template);

            var stepDocument = Mapper.Map<TemplateStepDocument>(step);
            stepDocument.TemplateId = templateId;
            return stepDocument;
        }

        [Transaction]
        public TemplateStepDocument Patch(int templateId, Guid stepId, TemplateStepPatchRequest request)
        {
            var template = templateResource.GetTemplate(templateId);
            var patch = Mapper.Map<TemplateStepPatch>(request);
            var step = template.UpdateStep(stepId, patch);
            
            templateRepository.Save(template);

            var stepDocument = Mapper.Map<TemplateStepDocument>(step);
            stepDocument.TemplateId = templateId;
            return stepDocument;
        }

        [Transaction]
        public void Delete(int templateId, Guid stepId)
        {
            var template = templateResource.GetTemplate(templateId);
            template.DeleteStep(stepId);
            templateRepository.Save(template);
        }

        public TemplateStepCollection List(int templateId)
        {
            var template = templateResource.GetTemplate(templateId);

            var steps = Mapper.Map<IList<TemplateStepDocument>>(template.Steps).ToList();
            steps.ForEach(step => step.TemplateId = templateId);
            return new TemplateStepCollection(steps)
            {
                TemplateId = templateId
            };
        }
    }
}