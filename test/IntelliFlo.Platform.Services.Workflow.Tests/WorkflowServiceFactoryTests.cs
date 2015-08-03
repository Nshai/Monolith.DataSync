using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Services.Workflow.Collaborators.v1;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1.Activities;
using Moq;
using NUnit.Framework;
using CreateTaskStep = IntelliFlo.Platform.Services.Workflow.Domain.CreateTaskStep;
using DelayStep = IntelliFlo.Platform.Services.Workflow.Domain.DelayStep;

namespace IntelliFlo.Platform.Services.Workflow.Tests
{
    [TestFixture]
    public class WorkflowServiceFactoryTests
    {
        private Mock<IServiceHttpClientFactory> clientFactory;
        private Mock<IServiceHttpClient> client;
        private IWorkflowServiceFactory underTest;
        private const int TenantId = 123;
        private const int OwnerId = 222;
        private const int OwnerPartyId = 948;

        [SetUp]
        public void SetUp()
        {
            clientFactory = new Mock<IServiceHttpClientFactory>();
            client = new Mock<IServiceHttpClient>();

            clientFactory.Setup(c => c.Create(It.IsAny<string>())).Returns(client.Object);
            client.Setup(c => c.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoByUserId, OwnerId), null)).Returns(Task.FromResult(new HttpResponse<Dictionary<string, object>>() {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new Dictionary<string, object> {{Principal.Constants.ApplicationClaimTypes.PartyId, OwnerPartyId}}}));
            
            underTest = new WorkflowServiceFactory(new DayDelayPeriod(), clientFactory.Object);
        }


        [Test]
        public void WhenBuildTemplateThenGeneratedSuccessfully()
        {
            var category = new TemplateCategory("Test", TenantId);
            var template = new Template("Test", TenantId, category, WorkflowRelatedTo.Client, OwnerId);
            template.AddStep(new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 101, 0, false, OwnerPartyId));
            template.AddStep(new DelayStep(Guid.NewGuid(), 5, true));

            var xaml = underTest.Build(template);
            
            var templateDefinition = new TemplateDefinition()
            {
                Name = template.Name,
                Definition = xaml,
                TenantId = TenantId,
                Version = TemplateDefinition.DefaultVersion,
                DateUtc = DateTime.UtcNow
            };

            templateDefinition.Compile();

        }
    }
}
