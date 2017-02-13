using System.Net;
using Microservice.Workflow.SubSystemTests.Helpers;
using Microservice.Workflow.SubSystemTests.v1.Models;
using NUnit.Framework;
using Reassure;
using Reassure.OAuth;

namespace Microservice.Workflow.SubSystemTests.v1.TemplateCategory
{
    [TestFixture]
    public class GetTests : ApiTestBase
    {
        //[Test]
        //public void When_Get_Category_By_Id_Should_Return_Expected_Json_Data()
        //{
        //    Test.Api()
        //        .Given()
        //            .OAuth2BearerToken(Config.User1.GetAccessToken())
        //            .Header("Accept", "application/json")
        //        .When()
        //            .Get<TemplateCategoryDocument>("v1/templatecategories/1")
        //        .Then()
        //            .ExpectBody(t => t.Name == "System")
        //            .ExpectStatus(HttpStatusCode.OK)
        //            .ExpectHeader("Content-Type", "application/json; charset=utf-8")
        //            .ExpectReasonPhrase("OK")
        //        .Run();
        //}

        [Test]
        public void When_Get_NonExisting_Template_Should_Return_Http_404()
        {
            Test.Api()
                .Given()
                    .Header("Accept", "application/json")
                    .OAuth2BearerToken(Config.User1.GetAccessToken())
                .When()
                    .Get<string>("v1/templatecategories/77777")
                .Then()
                    .ExpectStatus(HttpStatusCode.NotFound)
                    .ExpectReasonPhrase("Template category not found")
                .Run();
        }
    }
}
