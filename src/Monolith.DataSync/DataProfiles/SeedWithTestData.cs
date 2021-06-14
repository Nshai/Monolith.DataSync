using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IntelliFlo.Platform.Database;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.NHibernate;
using IntelliFlo.Platform.Principal;
using Microservice.DataSync.Collaborators.v2;
using Microservice.DataSync.Domain;
using Moq;
using NHibernate.Linq;
using Constants = IntelliFlo.Platform.Database.Constants;

namespace Microservice.DataSync.DataProfiles
{
    public class SeedWithTestData : TaskBase<SeedWithTestData>
    {
        private readonly IReadWriteSessionFactoryProvider provider;
        private const int TenantId = 10155;
        private const int UserId = 28000;
        private const int RoleId = 10115;
        private const int PartyId = 3500000;
        private const int TaskTypeId = 57881;
        private readonly Mock<IHttpClientFactory> clientFactory = new Mock<IHttpClientFactory>();
        private readonly Mock<IHttpClient> client = new Mock<IHttpClient>();

        public SeedWithTestData(IReadWriteSessionFactoryProvider provider)
        {
            this.provider = provider;
        }

        public override object Execute(IDatabaseSettings settings)
        {
            var claimsIdentity = new IntelliFloClaimsIdentity("Test", "Bearer");
            claimsIdentity.AddClaim(new Claim("user_id", TenantId.ToString(CultureInfo.InvariantCulture)));
            claimsIdentity.AddClaim(new Claim("tenant_id", TenantId.ToString(CultureInfo.InvariantCulture)));
            Thread.CurrentPrincipal = new IntelliFloClaimsPrincipal(claimsIdentity);

            using (var session = provider.SessionFactory.OpenSession())
            using (var tx = session.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                var instance = session.Query<DataSyncRequest>().SingleOrDefault(t => t.TenantId == TenantId);
                if (instance == null)
                {
                    instance = new DataSyncRequest(); // add test data for sub system tests
                    session.Save(instance);
                }

               
                tx.Commit();
                return true;
            }
        }

        public override void Dispose() { }
    }
}
