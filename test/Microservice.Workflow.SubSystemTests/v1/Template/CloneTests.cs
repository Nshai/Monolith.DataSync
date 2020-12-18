using System;
using Microservice.Workflow.SubSystemTests.Helpers.Apis;
using Microservice.Workflow.SubSystemTests.v1.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Reassure;
using Reassure.Security;

namespace Microservice.Workflow.SubSystemTests.v1.Template
{
    [TestFixture]
    public class CloneTests : ApiTestBase
    {
        [Test]
        public void When_Clone_Template_Then_Copied_Successfully()
        {
            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api()
                .Given()
                    .OAuth2BearerToken(GetUserAccessToken())
                    .Header("Accept", "application/json")
                .Body(JObject.Parse($"{{ Name: \"{Guid.NewGuid()}\" }}"))
                .When()
                    .Post<TemplateDocument>($"/v1/templates/{template.Id}/clone")
                .Then()
                    .ExpectStatus(201)
                    .ExpectHeader("Content-Type", "application/json; charset=utf-8")
                    .ExpectReasonPhrase("Created")
                .Run();
        }

        [Test]
        public void When_Clone_Template_With_Duplicate_Name_Then_Should_Return_Http_400()
        {

            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api()
                .Given()
                    .OAuth2BearerToken(GetUserAccessToken())
                    .Header("Accept", "application/json")
                    .Body(JObject.Parse($"{{ Name: \"{template.Name}\" }}"))
                .When()
                    .Post<TemplateDocument>($"/v1/templates/{template.Id}/clone")
                .Then()
                    .ExpectStatus(400)
                    .ExpectReasonPhrase("Template name must be unique")
                .Run();
        }


        [Test]
        public void When_Clone_NonExisting_Template_Then_Should_Return_Http_400()
        {
            Test.Api()
                .Given()
                    .OAuth2BearerToken(GetUserAccessToken())
                    .Header("Accept", "application/json")
                    .Body(JObject.Parse("{ Name: 'Test' }"))
                .When()
                    .Post<TemplateDocument>("/v1/templates/999/clone")
                .Then()
                    .ExpectStatus(404)
                    .ExpectReasonPhrase("Template not found")
                .Run();
        }
    }
}
