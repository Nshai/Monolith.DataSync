using System;
using IdentityServer3.Core.Services.InMemory;
using Microservice.Workflow.SubSystemTests.v1.Models;
using Reassure;
using Reassure.OAuth;

namespace Microservice.Workflow.SubSystemTests.Helpers.Apis
{
    public static class TemplateCategoryExtensions
    {
        public static TemplateCategoryDocument CreateCategory(this ApiTestBuilder builder, InMemoryUser user)
        {
            var state = Test.Api()
                .Given().OAuth2BearerToken(user.GetAccessToken()).Body(new CreateTemplateCategoryRequest() { Name = Guid.NewGuid().ToString() })
                .When().Post<TemplateCategoryDocument>("v1/templatecategories")
                .Then().ExpectStatus(200)
                .Run();

            return state.Response.Body as TemplateCategoryDocument;
        }
    }
}
