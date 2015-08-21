using System.Net;
using IntelliFlo.Platform.Testing.Api;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1;
using Microservice.Workflow.v1.Contracts;
using Microservice.Workflow.v1.Controllers;
using Moq;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    public class TemplateControllerTests
    {
        private TemplateController underTest;
        private Mock<ITemplateResource> templateResource;
        private const int TenantId = 101;
        private TemplateDocument template;

        [SetUp]
        public void SetUp()
        {
            templateResource = new Mock<ITemplateResource>();

            template = new TemplateDocument();

            underTest = new TemplateController(templateResource.Object);
        }

        [Test]
        public void WhenGetThenReturnsOk()
        {
            Test.ThisApi(underTest)
                .GivenDependenciesAreSetupAs(() => templateResource.Setup(t => t.Get(It.IsAny<int>())).Returns(template))
                .When(c => c.Get(123))
                .ThenExpectResponse(r => r.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void WhenTemplateDoesNotExistThenReturnsNotFound()
        {
            Test.ThisApi(underTest)
                .GivenDependenciesAreSetupAs(() => templateResource.Setup(t => t.Get(It.IsAny<int>())).Throws<TemplateNotFoundException>())
                .When(c => c.Get(123))
                .ThenExpectResult(r => r.Response.StatusCode == HttpStatusCode.NotFound);
        }
    }
}
