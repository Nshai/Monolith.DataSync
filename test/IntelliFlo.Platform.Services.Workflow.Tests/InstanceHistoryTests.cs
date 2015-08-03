using System;
using IntelliFlo.Platform.Services.Workflow.Domain;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IntelliFlo.Platform.Services.Workflow.Tests
{
    [TestFixture]
    public class InstanceHistoryTests
    {
        [Test]
        public void WhenSerializeThenSuccessful()
        {
            var instanceHistory = new InstanceHistory(new Guid("39DDA9C4-67B7-4144-A554-29400606A408"), new Guid("B790DFCD-F3B6-4204-802D-D7C6E9EB398E"), "Delay", new DateTime(2015, 7, 23))
            {
                Data = new LogData()
                {
                    Detail = new DelayLog()
                    {
                        DelayUntil = new DateTime(2015, 7, 30)
                    }
                }
            };

            var serialized = JsonConvert.SerializeObject(instanceHistory);
            Assert.AreEqual("{\"InstanceId\":\"39dda9c4-67b7-4144-a554-29400606a408\",\"StepId\":\"b790dfcd-f3b6-4204-802d-d7c6e9eb398e\",\"Step\":\"Delay\",\"Data\":{\"Detail\":{\"DelayUntil\":\"2015-07-30T00:00:00\",\"$key\":\"DelayLog\"}},\"IsComplete\":false,\"TimestampUtc\":\"2015-07-23T00:00:00\",\"Id\":0}", serialized);
        }
    }
}
