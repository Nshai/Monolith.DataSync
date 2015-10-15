using Microservice.Workflow.Domain;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class EnumExtensionTests
    {
        [TestCase(InstanceStatus.InProgress, ExpectedResult = "In Progress")]
        [TestCase(InstanceStatus.Aborted, ExpectedResult = "Aborted")]
        [TestCase(InstanceStatus.Completed, ExpectedResult = "Completed")]
        [TestCase(InstanceStatus.Errored, ExpectedResult = "Errored")]
        public string WhenFormatInstancestatusThenValueIsFormattedCorrectly(InstanceStatus status)
        {
            return status.ToPrettyString();
        }
    }
}
