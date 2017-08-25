﻿using System;
using System.Net;
using IdentityServer3.Core.Services.InMemory;
using Microservice.Workflow.SubSystemTests.v1.Models;
using Reassure;
using Reassure.OAuth;
using Reassure.Stubs;

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

        public static TemplateDocument CreateActiveTemplateWithExistingCategoryAndCreateTaskStep(this ApiTestBuilder builder, InMemoryUser user, int ownerPartyId, int templateRoleId)
        {
            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api().AddCreateTaskStep(Config.User1, template.Id, 3500000);
            Test.Api().AssignTemplateRole(Config.User1, template.Id, templateRoleId);

            var partyIdStub = Stub.Api()
                .Request().WithMethod("GET").WithUrl(url => url.Matching("/crm/v1/claims/user/.*"))
                .Return().WithBody($"{{'party_id': {ownerPartyId}}}").WithHeader("Content-Type", "application/json");

            using (partyIdStub.Setup())
            {
                Test.Api().MakeTemplateActive(Config.User1, template.Id);
            }

            return template;
        }

        public static void AssignTemplateRole(this ApiTestBuilder builder, InMemoryUser user, int templateId, int templateRoleId)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder
                .Given()
                .OAuth2BearerToken(user.GetAccessToken())
                .Header("Accept", "application/json")
                .Body(new[] { templateRoleId })
                .When()
                .Put<TemplateRoleDocument[]>($"/v1/templates/{templateId}/roles")
                .Then()
                .ExpectStatus(HttpStatusCode.OK)
                .Run();
        }

        public static void AddCreateTaskStep(this ApiTestBuilder builder, InMemoryUser user, int templateId, int assignedToPartyId)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder
                .Given()
                .OAuth2BearerToken(user.GetAccessToken())
                .Header("Accept", "application/json")
                .Body(new CreateTemplateStepRequest() { Type = "CreateTask", TaskTypeId = 123, Transition = "OnCompletion", AssignedTo = "User", AssignedToPartyId = assignedToPartyId })
                .When()
                .Post<TemplateStepDocument>($"/v1/templates/{templateId}/steps")
                .Then()
                .ExpectStatus(HttpStatusCode.Created)
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
