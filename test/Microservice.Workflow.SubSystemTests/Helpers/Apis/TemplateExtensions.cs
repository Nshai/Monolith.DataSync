using System;
using System.Net;
using IdentityServer3.Core.Services.InMemory;
using Microservice.Workflow.SubSystemTests.v1.Models;
using Reassure;
using Reassure.OAuth;

namespace Microservice.Workflow.SubSystemTests.Helpers.Apis
{
    public static class TemplateExtensions
    {
        public static TemplateDocument CreateTemplateWithExistingCategory(this ApiTestBuilder builder, InMemoryUser user)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            var state = builder
                .Given()
                .OAuth2BearerToken(user.GetAccessToken())
                .Body(new CreateTemplateRequest { Name = Guid.NewGuid().ToString(), RelatedTo = "Client", TemplateCategoryId = Config.ExistingCategoryId, OwnerUserId = Config.User1Id })
                .When()
                .Post<TemplateDocument>("/v1/templates")
                .Then()
                .ExpectStatus(HttpStatusCode.Created)
                .Run();

            return (TemplateDocument)state.Response.Body;
        }
    }
}
