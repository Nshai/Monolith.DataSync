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

        public static TemplateDocument CreateTemplateWithExistingCategoryAndMakeActive(this ApiTestBuilder builder, InMemoryUser user)
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

            var template = (TemplateDocument)state.Response.Body;
            Test.Api().AssignTemplateRole(user, template.Id);
            Test.Api().MakeTemplateActive(user, template.Id);

            return template;
        }

        public static void AssignTemplateRole(this ApiTestBuilder builder, InMemoryUser user, int templateId)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder
                .Given()
                .OAuth2BearerToken(user.GetAccessToken())
                .Header("Accept", "application/json")
                .Body(new[] { Config.RoleId })
                .When()
                .Put<TemplateRoleDocument[]>($"/v1/templates/{templateId}/roles")
                .Then()
                .ExpectStatus(HttpStatusCode.OK)
                .Run();
        }

        public static void MakeTemplateActive(this ApiTestBuilder builder, InMemoryUser user, int templateId)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder
                .Given()
                .OAuth2BearerToken(user.GetAccessToken())
                .Body(new TemplatePatchRequest() { Status = "Active"})
                .When()
                .Patch<TemplateDocument>($"/v1/templates/{templateId}")
                .Then()
                .ExpectStatus(HttpStatusCode.OK)
                .Run();
        }
    }
}
