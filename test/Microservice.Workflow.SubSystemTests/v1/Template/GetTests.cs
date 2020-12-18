using System;
using System.Net;
using Microservice.Workflow.SubSystemTests.Helpers;
using Microservice.Workflow.SubSystemTests.Helpers.Apis;
using Microservice.Workflow.SubSystemTests.v1.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Reassure;
using Reassure.Security;

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
                .Given()
                    .OAuth2BearerToken(GetUserAccessToken())
                    .Header("Accept", "application/json")
                .When()
                    .Get<TemplateDocument>($"v1/templates/{template.Id}")
                .Then()
                    .ExpectBody(t => t.Name == template.Name)
                    .ExpectStatus(HttpStatusCode.OK)
                    .ExpectHeader("Content-Type", "application/json; charset=utf-8")
                    .ExpectReasonPhrase("OK")
                .Run();
        }

        [Test]
        public void When_Get_NonExisting_Template_Should_Return_Http_404()
        {
            Test.Api()
                .Given()
                    .Header("Accept", "application/json")
                    .OAuth2BearerToken(GetUserAccessToken())
                .When()
                    .Get<string>("v1/templates/999")
                .Then()
                    .ExpectStatus(HttpStatusCode.NotFound)
                    .ExpectReasonPhrase("Template not found")
                .Run();
        }

        [Test]
        public void When_Get_Template_By_Name_Should_Return_Expected_Json_Data()
        {
            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api()
                .Given()
                    .OAuth2BearerToken(GetUserAccessToken())
                    .Header("Accept", "application/json")
                .When()
                    .Get<TemplateCollection>($"v1/templates?$filter=Name eq '{template.Name}'")
                .Then()
                    .ExpectBody(t => t.Items[0].Name == template.Name)
                    .ExpectStatus(HttpStatusCode.OK)
                    .ExpectHeader("Content-Type", "application/json; charset=utf-8")
                    .ExpectReasonPhrase("OK")
                .Run();
        }

        [Test]
        public void When_Get_Template_By_CategoryName_Should_Return_Expected_Json_Data()
        {
            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api()
                .Given()
                    .OAuth2BearerToken(GetUserAccessToken())
                    .Header("Accept", "application/json")
                .Body(JObject.Parse($"{{ Name: \"{Guid.NewGuid()}\" }}"))
                .When().Post<TemplateDocument>($"v1/templates/{template.Id}/clone")
                .Then()
                    .ExpectStatus(201)
                    .ExpectHeader("Content-Type", "application/json; charset=utf-8")
                    .ExpectReasonPhrase("Created")
                .Run();
        }
    }
}