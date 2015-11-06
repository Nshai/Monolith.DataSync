using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using AutoMapper;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Principal;
using IntelliFlo.Platform.Transactions;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1.Resources
{
    public partial class TemplateResource
    {
        public void CreateInstance(string templateIdentifier, CreateInstanceRequest request)
        {
            Check.IsNotNull(request, "Request was not supplied");

            var tenantId = Thread.CurrentPrincipal.AsIFloPrincipal().TenantId;

            var registration = templateRegistrationRepository.Query().Where(t => t.Identifier == templateIdentifier && (t.TenantId == tenantId || t.TenantId == 0)).OrderByDescending(t => t.TenantId).FirstOrDefault();
            if(registration == null)
                throw new TemplateNotFoundException();

            var bearerToken = tokenBuilder.Build(DateTime.UtcNow, ClaimsPrincipal.Current);

            workflowHost.CreateAsync(registration.Template, new WorkflowContext
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

        [Transaction]
        public TemplateRegistrationDocument Register(string identifier, RegisterTemplateRequest request)
        {
            var tenantId = Thread.CurrentPrincipal.AsIFloPrincipal().TenantId;

            var templateDefinition = templateDefinitionRepository.Get(request.TemplateId);
            if(templateDefinition == null)
                throw new TemplateNotFoundException();

            var existingRegistration = templateRegistrationRepository.Query().SingleOrDefault(r => r.TenantId == tenantId && r.Identifier == identifier);
            if(existingRegistration != null)
                templateRegistrationRepository.Delete(existingRegistration);

            var registration = new TemplateRegistration(identifier, templateDefinition);
            templateRegistrationRepository.Save(registration);

            return Mapper.Map<TemplateRegistrationDocument>(registration);
        }

        [Transaction]
        public void Unregister(string identifier)
        {
            var tenantId = Thread.CurrentPrincipal.AsIFloPrincipal().TenantId;

            var existingRegistration = templateRegistrationRepository.Query().SingleOrDefault(r => r.TenantId == tenantId && r.Identifier == identifier);
            if (existingRegistration != null)
                templateRegistrationRepository.Delete(existingRegistration);
        }
    }
}