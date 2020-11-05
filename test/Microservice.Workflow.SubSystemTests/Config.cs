using NUnit.Framework;
using Reassure;
using Reassure.Bus;
using Reassure.Bus.Services;
using Reassure.Security;
using Reassure.Stubs;
using Reassure.Stubs.Providers;

namespace Microservice.Workflow.SubSystemTests
{
    [SetUpFixture]
    public class Config : CoverageSetUp
    {
        public static readonly int ExistingCategoryId = 1;
        public static readonly int MasterTenantId = 10155;
        public static readonly int TenantAId = 888;
        public static readonly int TenantBId = 999;
        public static readonly int ClientId = 6180642;
        public static readonly int User1Id = 111;
        public static readonly int RoleId1 = 123;
        public static readonly int RoleId2 = 123444;
        public static readonly int RoleId3 = 55222;
        public static readonly int Party1Id = 222;
        public static readonly string Subject = "d6e9a9d8-6a35-499c-a629-a4e600bcd2ac";


        public static TestUser User1 { get; private set; }

        [OneTimeSetUp]
        public void SetUp()
        {
            TokenBuilder.CertificateSubject = Configuration["AppSettings:Client.certificate.default.subjectname"];

            SetUpBusConfig();
            SetUpReassure();
            SetUpUsers();
        }


        private static void SetUpUsers()
        {
            User1 = new TestUser(User1Id, Party1Id, MasterTenantId, Subject);
        }

        private static void SetUpBusConfig()
        {
            BusConfig.Configure(new ServiceSettings
            {
                CertificateSubject = Configuration["AppSettings:Client.certificate.default.subjectname"],
                Region = Configuration["Reassure:Bus:Region"],
                Service = Configuration["Reassure:Bus:Service"],
                Evironment = Configuration["Reassure:Bus:Environment"],
                LogAddress = Configuration["Reassure:Bus:LogAddress"],
                Instance = Configuration["Reassure:Bus:Instance"],
                ProfileName = Configuration["Aws:Profile"],
                ProfilesLocation = Configuration["Aws:ProfilesLocation"]
            });
        }

        private static void SetUpReassure()
        {
            Test.BaseAddressProvider = () => Configuration["Reassure:ServiceBaseAddress"];

            Stub.DefaultProviderFactory = () => new WireMock(new WireMockConfiguration
            {
                BaseAddress = Configuration["Reassure:WiremockBaseAddress"]
            });
        }
    }
}