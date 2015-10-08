using System;
using Microservice.Workflow.SubSystemTests.Helpers;
using Microservice.Workflow.SubSystemTests.Helpers.Apis;
using Microservice.Workflow.SubSystemTests.v1.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Reassure;
using Reassure.OAuth;

namespace Microservice.Workflow.SubSystemTests.v1.Template
{
    [TestFixture]
    public class GetTests : ApiTestBase
    {
        [Test]
        public void When_Get_Existing_Template_Should_Return_Expected_Json_Data()
        {
            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api()
                .Given().OAuth2BearerToken(Config.User1.GetAccessToken())
                .When().Get<TemplateDocument>(string.Format("v1/templates/{0}", template.Id))
                .Then().ExpectBody(t => t.Name == template.Name)
                .Run();
        }

        [Test]
        public void When_Get_NonExisting_Template_Should_Return_Http_404()
        {
            Test.Api()
                .Given().OAuth2BearerToken(Config.User1.GetAccessToken())
                .When().Get<string>("v1/templates/999")
                .Then().ExpectStatus(404)
                .Run();
        }

        [Test]
        public void When_Get_Template_By_Name_Should_Return_Expected_Json_Data()
        {
            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api()
                .Given().OAuth2BearerToken(Config.User1.GetAccessToken())
                .When().Get<TemplateCollection>(string.Format("v1/templates?$filter=Name eq '{0}'", template.Name))
                .Then().ExpectBody(t => t.Items[0].Name == template.Name)
                .Run();
        }

        [Test]
        public void When_Clone_Template_Then_Copied_Successfully()
        {
            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api()
                .Given()
                .OAuth2BearerToken(Config.User1.GetAccessToken())
                .Header("Accept", "application/json")
                .Body(JObject.Parse(string.Format("{{ Name: \"{0}\" }}", Guid.NewGuid())))
                .When().Post<TemplateDocument>(string.Format("v1/templates/{0}/clone", template.Id))
                .Then().ExpectStatus(201)
                .Run();
        }

        [Test]
        public void When_Clone_Template_With_Duplicate_Name_Then_Should_Return_Http_400()
        {
            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api()
                .Given()
                .OAuth2BearerToken(Config.User1.GetAccessToken())
                .Header("Accept", "application/json")
                .Body(JObject.Parse(string.Format("{{ Name: \"Test\" }}")))
                .When().Post<TemplateDocument>(string.Format("v1/templates/{0}/clone", template.Id))
                .Then().ExpectStatus(400).ExpectReasonPhrase("Template name must be unique")
                .Run();
        }


        [Test]
        public void When_Clone_NonExisting_Template_Then_Should_Return_Http_400()
        {
            Test.Api()
                .Given()
                .OAuth2BearerToken(Config.User1.GetAccessToken())
                .Header("Accept", "application/json")
                .Body(JObject.Parse(string.Format("{{ Name: \"Test\" }}")))
                .When().Post<Models.TemplateDocument>("v1/templates/999/clone")
                .Then().ExpectStatus(404)
                .Run();
        }

    }
}