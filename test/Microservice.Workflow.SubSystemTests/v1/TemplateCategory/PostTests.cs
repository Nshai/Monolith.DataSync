using Microservice.Workflow.SubSystemTests.v1.Models;
using NUnit.Framework;
using Reassure;
using Reassure.Security;

namespace Microservice.Workflow.SubSystemTests.v1.TemplateCategory
{
    public class PostTests : ApiTestBase
    {
        [Test]
        public void When_Valid_Category_Send_Should_Return_Expected_Json_Data()
        {
            Test.Api()
                .Given()
                    .OAuth2BearerToken(GetUserAccessToken())
                    .Header("Accept", "application/json")
                    .Body(new CreateTemplateCategoryRequest { Name ="Test template category" })
                .When()
                    .Post<TemplateCategoryDocument>("/v1/templatecategories")
                .Then()
                    .ExpectStatus(201)
                    .ExpectReasonPhrase("Created")
                    .ExpectHeader("Content-Type", "application/json; charset=utf-8")
                .Run();
        }
    }
}
