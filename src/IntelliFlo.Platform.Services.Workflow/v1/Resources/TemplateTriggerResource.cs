using System;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Services.Workflow.Collaborators.v1;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;
using IntelliFlo.Platform.Transactions;

namespace IntelliFlo.Platform.Services.Workflow.v1.Resources
{
    public class TemplateTriggerResource : ITemplateTriggerResource, IHandle<TemplateCloned>, IHandle<TemplateMadeActive>, IHandle<TemplateMadeInactive>
    {
        private readonly IRepository<Template> templateRepository;
        private readonly ITemplateResource templateResource;
        private readonly IServiceHttpClientFactory clientFactory;
        private readonly IServiceAddressRegistry serviceAddressRegistry;

        public TemplateTriggerResource(IRepository<Template> templateRepository, ITemplateResource templateResource, IServiceHttpClientFactory clientFactory, IServiceAddressRegistry serviceAddressRegistry)
        {
            this.templateRepository = templateRepository;
            this.templateResource = templateResource;
            this.clientFactory = clientFactory;
            this.serviceAddressRegistry = serviceAddressRegistry;
        }

        public TemplateTriggerCollection Get(int templateId)
        {
            var template = templateResource.GetTemplate(templateId);
            return Map(template);
        }

        [Transaction]
        public TemplateTriggerCollection Post(int templateId, CreateTemplateTrigger request)
        {
            var type = (TriggerType)Enum.Parse(typeof(TriggerType), request.Type);

            var template = templateResource.GetTemplate(templateId);
            template.SetTrigger(type, TemplateTriggerFactory.CreateFromRequest(request));
            templateRepository.Save(template);

            return Map(template);
        }

        private static TemplateTriggerCollection Map(Template template)
        {
            var triggerSet = template.TriggerSet;
            var trigger = new TemplateTrigger
            {
                TemplateId = template.Id,
                TemplateVersionId = template.CurrentVersion.Id,
                TriggerType = triggerSet.TriggerType.ToString()
            };
            triggerSet.Trigger.PopulateDocument(trigger);

            var triggerDocument = Mapper.Map<TemplateTriggerDocument>(trigger);

            return new TemplateTriggerCollection(new[] {triggerDocument})
            {
                TemplateId = template.Id,
                TemplateVersionId = template.CurrentVersion.Id
            };
        }

        [Transaction]
        public void Handle(TemplateCloned args)
        {
            var clonedFromTemplate = templateResource.GetTemplate(args.ClonedFromTemplateId);
            var template = templateResource.GetTemplate(args.TemplateId);
            template.SetTrigger(clonedFromTemplate.TriggerSet.TriggerType, clonedFromTemplate.TriggerSet.Trigger);

            templateRepository.Save(template);
        }

        [Transaction]
        public void Handle(TemplateMadeActive args)
        {
            var template = templateResource.GetTemplate(args.TemplateId);
            var triggerSet = template.TriggerSet;
            var trigger = triggerSet.Trigger;

            if (triggerSet.TriggerType == TriggerType.None)
                return;

            var baseAddress = serviceAddressRegistry.GetServiceEndpoint("workflow").BaseAddress;
            var callbackUrl = string.Format("{0}{1}", baseAddress, string.Format(Uris.Self.TriggerInstance, template.Id));

            using (var client = clientFactory.Create("eventmanagement"))
            {
                HttpResponse<EventSubscriptionDocument> subscribeResponse = null;
                var subscribeTask = client.Post<EventSubscriptionDocument, SubscribeRequest>(Uris.EventManagement.Post, new SubscribeRequest
                {
                    EventType = trigger.EventName,
                    EntityId = 0,
                    Filter = ODataBuilder.BuildExpression(trigger.GetFilter().ToList()),
                    CallbackUrl = callbackUrl,
                    IsPersistent = true
                }).ContinueWith(t =>
                {
                    t.OnException(s => { throw new HttpResponseException(s); });
                    subscribeResponse = t.Result;
                });
                subscribeTask.Wait();
                var subscription = subscribeResponse.Resource;

                triggerSet.EventSubscriptionId = subscription.SubscriptionId;

                templateRepository.Save(template);
            }
        }

        public void Handle(TemplateMadeInactive args)
        {
            var template = templateResource.GetTemplate(args.TemplateId);

            if (template.TriggerSet.EventSubscriptionId == 0)
                return;

            using (var client = clientFactory.Create("eventmanagement"))
            {
                var subscribeTask = client.Delete(string.Format(Uris.EventManagement.Delete, template.TriggerSet.EventSubscriptionId))
                    .ContinueWith(t => { t.OnException(s => { throw new HttpResponseException(s); }); });
                subscribeTask.Wait();
            }
        }
    }
}