using System;
using IntelliFlo.Platform.Services.Workflow.Domain;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IntelliFlo.Platform.Services.Workflow.Tests
{
    [TestFixture]
    public class WorkflowDefinitionTests
    {
        [Test]
        public void WhenUpdateStepsThenNotEqual()
        {
            var stepId = Guid.NewGuid();

            var original = new WorkflowDefinition();
            original.AddStep(new CreateTaskStep(stepId, TaskTransition.OnCompletion, 1011));

            var copy = new WorkflowDefinition();
            copy.AddStep(new CreateTaskStep(stepId, TaskTransition.OnCompletion, 1011));

            Assert.AreEqual(original, copy);

            copy = new WorkflowDefinition();
            copy.AddStep(new CreateTaskStep(stepId, TaskTransition.OnCompletion, 1011, 10));
            
            Assert.AreNotEqual(original, copy);
        }

        [Test]
        public void WhenSerializeThenSuccessful()
        {
            var stepId = new Guid("1FECE1BD-9001-403F-B280-B820DDDF0D94");
            var definition = new WorkflowDefinition();
            definition.AddStep(new DelayStep(stepId, 10, true));

            var serialized = JsonConvert.SerializeObject(definition);
            Assert.AreEqual("{\"Steps\":[{\"Id\":\"1fece1bd-9001-403f-b280-b820dddf0d94\",\"Days\":10,\"BusinessDays\":true,\"$key\":\"DelayStep\"}]}", serialized);
        }
    }
}
