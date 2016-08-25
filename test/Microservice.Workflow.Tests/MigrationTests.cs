using System;
using Microservice.Workflow.v1.Activities;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class MigrationTests
    {
        [Test]
        public void SerializeAdditionalContext()
        {
            var serialized = JsonConvert.SerializeObject(new AdditionalContext() { RunTo = new RunToAdditionalContext()
            {
                StepId = new Guid("b4f659ec-a373-4e20-9408-810289505c7f"), 
                TaskId = 123,
                DelayTime = new DateTime(2015, 6, 29, 9, 32, 0, DateTimeKind.Utc)
            } });
            Assert.AreEqual("{\"RunTo\":{\"StepId\":\"b4f659ec-a373-4e20-9408-810289505c7f\",\"StepIndex\":0,\"DelayTime\":\"2015-06-29T09:32:00Z\",\"TaskId\":123}}", serialized);
        }
    }
}
