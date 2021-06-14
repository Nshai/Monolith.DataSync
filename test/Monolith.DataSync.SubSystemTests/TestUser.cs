using System.Security.Claims;

namespace Monolith.DataSync.SubSystemTests
{
    public class TestUser
    {
        //string firstName, string lastName
        public TestUser(int? userId, int? partyId, int tenantId, string subject)
        {
            UserId = userId;
            PartyId = partyId;
            TenantId = tenantId;
            Subject = subject;

            Claims = new[]
            {
                    new Claim("srv_adviser_party_id", "1434450"),
                    new Claim("srv_adviser_subject", "954ddb40-8f75-4aa0-8e52-9fed0173ec95"),
                    new Claim("group_lineage", "159,1411"),
                    new Claim("pfp_premium", "True"),
                    new Claim("srv_adviser_name", "Elsa Admin"),
                    new Claim("tenant_guid", "e777a02e-a192-4323-85fd-98ed1ab18322"),
                    new Claim("iss", "https://idsrv3.com"),
                    new Claim("idp", "idsrv"),
                    new Claim("pfp_domain", "intellliflo99.mypfp.co.uk"),
                    new Claim("email_verified", "True"),
                    new Claim("group_id", "4000001"),
                    new Claim("role_ids", Config.RoleId1 + ", " + Config.RoleId2),
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
                };
        }

        public int? UserId { get; }
        public int TenantId { get; }
        public int? PartyId { get; }
        public string Subject { get; }
        public Claim[] Claims { get; }
    }
}
