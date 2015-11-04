using System.Threading.Tasks;
using Microservice.Workflow.SubSystemTests.Helpers;
using Microservice.Workflow.SubSystemTests.Helpers.Apis;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Reassure;
using Reassure.OAuth;
using Reassure.Stubs;

namespace Microservice.Workflow.SubSystemTests.v1.Template
{
    [TestFixture]
    public class CreateInstanceTests : ApiTestBase
    {
        [Test]
        public void When_Create_Instance_For_Draft_Template_Then_Should_Return_Http_400()
        {
            var template = Test.Api().CreateTemplateWithExistingCategory(Config.User1);
            Test.Api()
                .Given()
                .OAuth2BearerToken(Config.User1.GetAccessToken())
                .Header("Accept", "application/json")
                .Body(JObject.Parse("{ EntityType: \"Client\", EntityId: 1 }"))
                .When().Post<string>(string.Format("v1/templates/{0}/createinstance/ondemand", template.Id))
                .Then().ExpectStatus(400).ExpectReasonPhrase("Template not active")
                .Run();
        }

        [Test]
        public void When_Create_Instance_For_Active_Template_Then_Should_Return_Http_200()
        {
            var template = Test.Api().CreateActiveTemplateWithExistingCategoryAndCreateTaskStep(Config.User1, 3500000);

            var claimsStub = Stub.Api()
                .Request().WithMethod("GET").WithUrl(url => url.Matching("/crm/v1/claims/subject/.*"))
                .Return().WithBody("{'party_id': 3500000, 'role_id': 10115, 'group_lineage': '3522' }").WithHeader("Content-Type", "application/json");

            var taskStub = Stub.Api()
                .Request().WithMethod("POST").WithUrl(url => url.EqualTo("/crm/v1/tasks"))
                .Return().WithBody("{ TaskId: 123, AssignedToPartyId: 3500000 }").WithHeader("Content-Type", "application/json");

            var subscriptionStub = Stub.Api()
                .Request().WithMethod("POST").WithUrl(url => url.EqualTo("/eventmanagement/v1/subscriptions"))
                .Return().WithBody("{}").WithHeader("Content-Type", "application/json");

            using (claimsStub.Setup())
            {
                taskStub.Setup();
                subscriptionStub.Setup();

                Test.Api()
                    .Given()
                    .OAuth2BearerToken(Config.User1.GetAccessToken())
                    .Header("Accept", "application/json")
                    .Body(JObject.Parse("{ EntityType: \"Client\", EntityId: 1 }"))
                    .When().Post<string>(string.Format("v1/templates/{0}/createinstance/ondemand", template.Id))
                    .Then().ExpectStatus(204)
                    .Run();

                // TODO Replace this with wait until instance is unloaded?
                Task.Delay(10000).Wait();

                taskStub.Verify().IsCalled(Times.Once());
                subscriptionStub.Verify().IsCalled(Times.Twice());
            }
        }
    }
}