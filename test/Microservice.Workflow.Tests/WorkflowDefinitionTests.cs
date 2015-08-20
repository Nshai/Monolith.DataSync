using System;
using Microservice.Workflow.Domain;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class WorkflowDefinitionTests
    {


        [Test]
        public void WhenUpdateStepsThenNotEqual()
        {
            var stepId = Guid.NewGuid();
            var partyId = 123;

            var original = new WorkflowDefinition();
            original.AddStep(new CreateTaskStep(stepId, TaskTransition.OnCompletion, 1011, TaskAssignee.User, assignedToPartyId: partyId));

            var copy = new WorkflowDefinition();
            copy.AddStep(new CreateTaskStep(stepId, TaskTransition.OnCompletion, 1011, TaskAssignee.User, assignedToPartyId: partyId));

            Assert.AreEqual(original, copy);

            copy = new WorkflowDefinition();
            copy.AddStep(new CreateTaskStep(stepId, TaskTransition.OnCompletion, 1011, TaskAssignee.User, assignedToPartyId: 10));
            
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

        [TestCase("{\"Steps\":[{\"Id\":\"10a983a2-7444-4b4e-b26c-2aba3e50ff44\",\"Transition\":2,\"TaskTypeId\":1011,\"DueDelay\":0,\"DueDelayBusinessDays\":false,\"AssignedToPartyId\":1,\"AssignedToRoleId\":null,\"AssignedToRoleContext\":null}]}", ExpectedResult = TaskAssignee.User)]
        [TestCase("{\"Steps\":[{\"Id\":\"10a983a2-7444-4b4e-b26c-2aba3e50ff44\",\"Transition\":2,\"TaskTypeId\":1011,\"DueDelay\":0,\"DueDelayBusinessDays\":false,\"AssignedToPartyId\":null,\"AssignedToRoleId\":123,\"AssignedToRoleContext\":null}]}", ExpectedResult = TaskAssignee.Role)]
        [TestCase("{\"Steps\":[{\"Id\":\"10a983a2-7444-4b4e-b26c-2aba3e50ff44\",\"Transition\":2,\"TaskTypeId\":1011,\"DueDelay\":0,\"DueDelayBusinessDays\":false,\"AssignedToPartyId\":null,\"AssignedToRoleId\":null,\"AssignedToRoleContext\":123}]}", ExpectedResult = TaskAssignee.ContextRole)]
        [TestCase("{\"Steps\":[{\"Id\":\"10a983a2-7444-4b4e-b26c-2aba3e50ff44\",\"Transition\":2,\"TaskTypeId\":1011,\"DueDelay\":0,\"DueDelayBusinessDays\":false,\"AssignedToPartyId\":null,\"AssignedToRoleId\":null,\"AssignedToRoleContext\":null}]}", ExpectedResult = null)]
        public TaskAssignee? WhenSerializeStepWithoutAssignedToThenCorrectValueIsInferred(string serialization)
        {
            var deserialized = JsonConvert.DeserializeObject<WorkflowDefinition>(serialization);
            var createTaskStep = deserialized.Steps[0] as CreateTaskStep;
            return createTaskStep.AssignedTo;
        }
    }
}
