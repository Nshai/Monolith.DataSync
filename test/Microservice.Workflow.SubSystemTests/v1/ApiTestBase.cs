using NUnit.Framework;
using Reassure.Stubs;

namespace Microservice.Workflow.SubSystemTests.v1
{
    public class ApiTestBase
    {
        [TearDown]
        public virtual void TearDown()
        {
            Stub.DefaultProviderFactory().Reset();
        }
    }
}
