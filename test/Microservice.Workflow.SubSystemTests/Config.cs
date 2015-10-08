using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;
using log4net.Config;
using NUnit.Framework;
using Reassure;
using Reassure.OAuth;
using Reassure.Stubs;
using Reassure.Stubs.Providers;

namespace Microservice.Workflow.SubSystemTests
{
    [SetUpFixture]
    public class Config
    {
        public static readonly int ExistingCategoryId = 1;
        public static readonly int MasterTenantId = 10155;
        public static readonly int TenantAId = 888;
        public static readonly int TenantBId = 999;
        public static readonly int ClientId = 6180642;
        public static readonly int User1Id = 111;

        private static Client[] allClients;

        public static OAuth2Server Server
        {
            get;
            private set;
        }

        public static Client Client
        {
            get;
            private set;
        }

        public static InMemoryUser User2
        {
            get;
            set;
        }

        public static InMemoryUser User1
        {
            get;
            private set;
        }

        public static InMemoryUser TenantAUser
        {
            get;
            private set;
        }

        public static InMemoryUser TenantBUser
        {
            get;
            private set;
        }

        [SetUp]
        public void SetUp()
        {
            XmlConfigurator.Configure();

            Test.BaseAddressProvider = () => ConfigurationManager.AppSettings["Reassure:ServiceBaseAddress"];

            allClients = Clients.Get().ToArray();
            Client = allClients.First(c => c.Flow == Flows.ResourceOwner);
            Client.AllowedScopes.Add("workflow");

            User1 = CreateUser("Vasily", "Pupkin", User1Id, 222, MasterTenantId, "d6e9a9d8-6a35-499c-a629-a4e600bcd2ac");
            User2 = CreateUser("Ivan", "Ivanov", 444, 555, MasterTenantId, "d6e9a9d8-6a35-499c-a629-a4e600bcd3ac");
            TenantAUser = CreateUser("Vlas", "Vlasov", 666, 777, TenantAId, "d6e9a9d8-6a35-499c-a629-a4e600bcd4ac");
            TenantBUser = CreateUser("Kisa", "Vorobyaninov", 555, 444, TenantBId, "d6e9a9d8-6a35-499c-a629-a4e600bcd5ac");

            var scopes = Scopes.Get().ToList();
            scopes.Add(StandardScopes.AllClaims);

            Server = OAuth2Server.Create(
                () => new[] { User1, User2, TenantAUser, TenantBUser },
                () => allClients,
                () => scopes.ToArray(),
                Cert.LoadEmbedded);

            var wireMock = new WireMock(new WireMockConfiguration
            {
                BaseAddress = ConfigurationManager.AppSettings["Reassure:WiremockBaseAddress"]
            });
            Stub.DefaultProviderFactory = () => wireMock;
        }

        private static InMemoryUser CreateUser(
            string firstName,
            string lastName,
            long userId,
            long partyId,
            long tenantId,
            string subject)
        {
            return new InMemoryUser
            {
                Subject = subject,
                Username = firstName + lastName,
                Password = "Password123",
                Enabled = true,
                Claims = new[]
                {
                    new Claim("srv_adviser_party_id", "1434450"),
                    new Claim("srv_adviser_subject", "954ddb40-8f75-4aa0-8e52-9fed0173ec95"),
                    new Claim("group_lineage", "159,1411"),
                    new Claim("user_id", userId.ToString()),
                    new Claim("sub", subject),
                    new Claim("given_name", firstName),
                    new Claim("email", firstName + "." + lastName + "@google.com"),
                    new Claim("pfp_premium", "True"),
                    new Claim("username", firstName + lastName),
                    new Claim("srv_adviser_name", "Elsa Admin"),
                    new Claim("party_id", partyId.ToString()),
                    new Claim("tenant_guid", "e777a02e-a192-4323-85fd-98ed1ab18322"),
                    new Claim("family_name", lastName),
                    new Claim("iss", "https://idsrv3.com"),
                    new Claim("tenant_id", tenantId.ToString()),
                    new Claim("idp", "idsrv"),
                    new Claim("pfp_domain", "intellliflo99.mypfp.co.uk"),
                    new Claim("email_verified", "True"),
                    new Claim("group_id", "4000001"),

                    new Claim("scope", "openid"),
                    new Claim("scope", "profile"),
                    new Claim("scope", "email"),
                    new Claim("scope", "phone"),
                    new Claim("scope", "offline_access"),
                    new Claim("scope", "crm"),
                    new Claim("scope", "factfind"),
                    new Claim("scope", "portfolio"),
                    new Claim("scope", "storage"),
                    new Claim("scope", "securemessaging"),
                    new Claim("scope", "insight"),
                    new Claim("scope", "payment"),
                    new Claim("scope", "brand"),
                    new Claim("scope", "catalog"),
                    new Claim("scope", "applicationstore"),
                    new Claim("scope", "pfp"),
                    new Claim("scope", "myprofile"),
                    new Claim("scope", "workflow")
                }

            };
        }

    }
}