using System.Net;
using NUnit.Framework;
using Reassure;
using Reassure.Security;

namespace Microservice.Workflow.SubSystemTests.v1.TemplateCategory
{
    [TestFixture]
    public class GetTests : ApiTestBase
    {
        [Test]
        public void When_Get_NonExisting_Template_Should_Return_Http_404()
        {
            Test.Api()
                .Given()
                    .Header("Accept", "application/json")
                    .OAuth2BearerToken(GetUserAccessToken())
                .When()
                    .Get<string>("v1/templatecategories/77777")
                .Then()
                    .ExpectStatus(HttpStatusCode.NotFound)
                    .ExpectReasonPhrase("Template category not found")
                .Run();
        }
    }
}
