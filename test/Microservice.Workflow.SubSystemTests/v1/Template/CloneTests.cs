using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class CloneTests : ApiTestBase
    {
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
                .Body(JObject.Parse($"{{ Name: \"{template.Name}\" }}"))
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
