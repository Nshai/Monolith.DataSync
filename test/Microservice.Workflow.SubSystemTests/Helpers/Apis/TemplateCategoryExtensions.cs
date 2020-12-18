using System;
using Microservice.Workflow.SubSystemTests.v1;
using Microservice.Workflow.SubSystemTests.v1.Models;
using Reassure;
using Reassure.Security;

namespace Microservice.Workflow.SubSystemTests.Helpers.Apis
{
    public static class TemplateCategoryExtensions
    {
        public static TemplateCategoryDocument CreateCategory(this ApiTestBuilder builder, TestUser user)
        {
               var state =
                Test.Api()
                    .Given()
                        .OAuth2BearerToken(ApiTestBase.GetUserAccessToken())
                    .Body(new CreateTemplateCategoryRequest() { Name = Guid.NewGuid().ToString() })
                    .When()
                        .Post<TemplateCategoryDocument>("/v1/templatecategories")
                    .Then()
                        .ExpectStatus(200)
                        .ExpectReasonPhrase("Created")
                        .ExpectHeader("Content-Type", "application/json; charset=utf-8")
                    .Run();

            return state.Response.Body as TemplateCategoryDocument;
        }
    }
}
