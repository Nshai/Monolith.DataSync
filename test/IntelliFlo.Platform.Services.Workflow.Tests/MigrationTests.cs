using System;
using IntelliFlo.Platform.Services.Workflow.v1.Activities;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IntelliFlo.Platform.Services.Workflow.Tests
{
    [TestFixture]
    public class MigrationTests
    {
        [Test]
        public void SerializeAdditionalContext()
        {
            var serialized = JsonConvert.SerializeObject(new AdditionalContext() { RunTo = new RunToAdditionalContext()
            {
                StepIndex = 4, 
                TaskId = 123,
                DelayTime = new DateTime(2015, 6, 29, 9, 32, 0, DateTimeKind.Utc)
            } });
            Assert.AreEqual("{\"RunTo\":{\"StepIndex\":4,\"DelayTime\":\"2015-06-29T09:32:00Z\",\"TaskId\":123}}", serialized);
        }
    }
}
