using NUnit.Framework;
using Reassure.Security;
using Reassure.Stubs;
using System.Linq;

namespace Microservice.Workflow.SubSystemTests.v1
{
    public abstract class ApiTestBase
    {
    
        [TearDown]
        public virtual void TearDown()
        {
            Stub.DefaultProviderFactory().Reset().Wait();
        }

        public static string GetUserAccessToken()
        {
            var tokenBuilder = new TokenBuilder()
                .Subject(Config.Subject)
                .UserId(Config.User1Id)
                .PartyId(Config.Party1Id)
                .TenantId(Config.MasterTenantId);


            foreach(var claim in Config.User1.Claims.Where(c => c.Type != "scope"))
            {
                tokenBuilder.Claim(claim.Type, claim.Value);
            }

            foreach(var scope in Config.User1.Claims.Where(c => c.Type =="scope"))
            {
                tokenBuilder.Scope(scope.Value);
            }

            return tokenBuilder.Build();
        }

    }
}
