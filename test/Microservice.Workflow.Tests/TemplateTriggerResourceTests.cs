using System.Linq;
using Autofac;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.NHibernate.Repositories;
using Microservice.Workflow.Domain;
using Microservice.Workflow.Modules;
using Microservice.Workflow.v1;
using Microservice.Workflow.v1.Resources;
using Moq;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class TemplateTriggerResourceTests
    {
        private const int TenantId = 101;
        private const int OwnerUserId = 123;
        private Template template;
        private const int UserId = 1;
        private readonly Mock<IRepository<Template>> templateRepository = new Mock<IRepository<Template>>();
        private readonly Mock<ITemplateResource> templateResource = new Mock<ITemplateResource>();
        private readonly Mock<IServiceHttpClientFactory> clientFactory = new Mock<IServiceHttpClientFactory>();
        private readonly Mock<IServiceAddressRegistry> addressRegistry = new Mock<IServiceAddressRegistry>();
        private ITemplateTriggerResource underTest;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            Microservice.Workflow.IoC.Initialize(builder.Build());
            
            underTest = new TemplateTriggerResource(templateRepository.Object, templateResource.Object, clientFactory.Object, addressRegistry.Object);
            
            new WorkflowAutoMapperModule().Load();
        }

        [Test]
        public void WhenRetrieveClientCreationTriggerThenRetrievedSuccessfully()
        {
            SetupTemplate(TriggerType.OnClientCreation, new ClientCreatedTrigger() { ClientCategories = new[] { 1, 2, 3 }, ClientStatusId = 4 }, WorkflowRelatedTo.Client);

            var triggerCollection = underTest.Get(1);

            var trigger = triggerCollection.Items.First();

            Assert.AreEqual("OnClientCreation", trigger.TriggerType);
            Assert.AreEqual(new[] { 1, 2, 3 }, trigger.ClientCategories);
            Assert.AreEqual(4, trigger.ClientStatusId);
        }

        [Test]
        public void WhenRetrievePlanCreationTriggerThenRetrievedSuccessfully()
        {
            SetupTemplate(TriggerType.OnPlanCreation, new PlanCreatedTrigger() { PlanProviders = new[] { 1, 2 }, PlanTypes = new[] { 3, 4 }, IsPreExisting = false }, WorkflowRelatedTo.Plan);

            var triggerCollection = underTest.Get(1);

            var trigger = triggerCollection.Items.First();

            Assert.AreEqual("OnPlanCreation", trigger.TriggerType);
            Assert.AreEqual(new[] { 1, 2 }, trigger.PlanProviders);
            Assert.AreEqual(new[] { 3, 4 }, trigger.PlanTypes);
            Assert.IsFalse(trigger.IsPreExisting.Value);
        }

        [Test]
        public void WhenRetrieveServiceCaseCreationTriggerThenRetrievedSuccessfully()
        {
            SetupTemplate(TriggerType.OnServiceCaseCreation, new ServiceCaseCreatedTrigger() { ServiceCaseCategories = new[] { 1, 2 }}, WorkflowRelatedTo.ServiceCase);

            var triggerCollection = underTest.Get(1);

            var trigger = triggerCollection.Items.First();

            Assert.AreEqual("OnServiceCaseCreation", trigger.TriggerType);
            Assert.AreEqual(new[] { 1, 2 }, trigger.ServiceCaseCategories);
        }

        private void SetupTemplate<T>(TriggerType type, T trigger, WorkflowRelatedTo relatedTo) where T : BaseTrigger
        {
            var category = new TemplateCategory("Test", TenantId);
            template = new Template("My template", TenantId, category, relatedTo, OwnerUserId);
            template.SetTrigger(type, trigger);

            templateResource.Setup(t => t.GetTemplate(It.IsAny<int>())).Returns(template);
        }
    }
}
