using System.Net;
using Microservice.Workflow.SubSystemTests.Helpers;
using Microservice.Workflow.SubSystemTests.v1.Models;
using NUnit.Framework;
using Reassure;
using Reassure.OAuth;

namespace Microservice.Workflow.SubSystemTests.v1.TemplateCategory
{
    public class DeleteTests : ApiTestBase
    {
        [Test]
        public void When_Category_Exists_Should_Delete()
        {
            var result = Test.Api()
                .Given()
                    .OAuth2BearerToken(Config.User1.GetAccessToken())
                    .Header("Accept", "application/json")
                    .Body(new CreateTemplateCategoryRequest { Name = "Test template category for removal" })
                .When()
                    .Post<TemplateCategoryDocument>("v1/templatecategories")
                .Then()
                    .ExpectStatus(201)
                    .ExpectReasonPhrase("Created")
                    .ExpectHeader("Content-Type", "application/json; charset=utf-8")
                .Run();

            var templateCategoryId =  ((TemplateCategoryDocument)result.Response.Body).TemplateCategoryId;

            Test.Api()
                .Given()
                    .OAuth2BearerToken(Config.User1.GetAccessToken())
                    .Header("Accept", "application/json")                  
                .When()
                    .Delete<string>($"v1/templatecategories/{templateCategoryId}")
                .Then()
                    .ExpectStatus(HttpStatusCode.NoContent)
                    .ExpectReasonPhrase("No Content")
                .Run();
        }
    }
}
