using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using Microservice.Workflow.Collaborators.v1;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Activities;
using Moq;
using NUnit.Framework;
using CreateTaskStep = Microservice.Workflow.Domain.CreateTaskStep;
using DelayStep = Microservice.Workflow.Domain.DelayStep;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class WorkflowServiceFactoryTests
    {
        private Mock<IHttpClientFactory> clientFactory;
        private Mock<IHttpClient> client;
        private IWorkflowServiceFactory underTest;
        private const int TenantId = 123;
        private const int OwnerId = 222;
        private const int OwnerPartyId = 948;

        [SetUp]
        public void SetUp()
        {
            clientFactory = new Mock<IHttpClientFactory>();
            client = new Mock<IHttpClient>();

            clientFactory.Setup(c => c.Create(It.IsAny<string>())).Returns(client.Object);
            client.Setup(c => c.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoByUserId, OwnerId), null)).Returns(Task.FromResult(new HttpResponse<Dictionary<string, object>>() {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new Dictionary<string, object> {{IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.PartyId, OwnerPartyId}}}));
            
            underTest = new WorkflowServiceFactory(new DayDelayPeriod(), clientFactory.Object);
        }


        [Test]
        public void WhenBuildTemplateThenGeneratedSuccessfully()
        {
            var category = new TemplateCategory("Test", TenantId);
            var template = new Template("Test", TenantId, category, WorkflowRelatedTo.Client, OwnerId);
            template.AddStep(new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 101, TaskAssignee.User, OwnerPartyId));
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
