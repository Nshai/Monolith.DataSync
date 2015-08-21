using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Autofac;
using IntelliFlo.Platform.Principal;
using Microservice.Workflow.Domain;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class TemplateTests
    {
        [SetUp]
        public void SetUp()
        {
            var category = new TemplateCategory("Test", TenantId);
            template = new Template("My template", TenantId, category, WorkflowRelatedTo.Client, OwnerUserId) {Id = TemplateId};

            var builder = new ContainerBuilder();
            Microservice.Workflow.IoC.Initialize(builder.Build());

            var identity = new IntelliFloClaimsIdentity("Bob", "Basic");
            identity.AddClaim(new Claim(Constants.ApplicationClaimTypes.UserId, UserId.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(Constants.ApplicationClaimTypes.TenantId, TenantId.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(Constants.ApplicationClaimTypes.Subject, Guid.NewGuid().ToString()));
            Thread.CurrentPrincipal = new IntelliFloClaimsPrincipal(identity);
        }

        private const int TenantId = 101;
        private const int OwnerUserId = 123;
        private Template template;
        private const int UserId = 1;
        private const int TemplateId = 999;

        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public void WhenUpdateNameThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;
            template.Name = "My template 2";

            Assert.IsTrue(template.Name == "My template 2", "Name should be set correctly");
        }

        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Draft, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public void WhenAddStepThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;

            template.AddStep(new CreateTaskStep(Guid.NewGuid(), TaskTransition.Immediately, 1));

            Assert.AreEqual(1, template.Steps.Count);
        }

        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Draft, true, ExpectedException = typeof(TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof(TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof(TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof(TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof(TemplateNotUpdatableException))]
        public void WhenUpdateStepThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            var step = new CreateTaskStep(Guid.NewGuid(), TaskTransition.Immediately, 1);
            template.AddStep(step);

            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;

            template.UpdateStep(step.Id, new TemplateStepPatch() {Transition = TaskTransition.OnCompletion.ToString()});
        }

        [TestCase(WorkflowStatus.Draft, false, ExpectedResult = "TemplateGroupUpdated")]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public string WhenUpdateApplicableToGroupIdThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;
            template.ApplicableToGroupId = 101;

            Assert.IsTrue(template.ApplicableToGroupId == 101, "ApplicableToGroupId should be set correctly");

            return string.Join(",", template.Events.Select(e => e.GetType().Name));
        }


        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public void WhenUpdateCategoryThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;
            template.Category = new TemplateCategory("WhenUpdateCategoryThenVerifyExpectedBehaviour", TenantId);

            Assert.AreEqual("WhenUpdateCategoryThenVerifyExpectedBehaviour", template.Category.Name, "Category should be set correctly");
        }

        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Active, false)]
        [TestCase(WorkflowStatus.Active, true)]
        [TestCase(WorkflowStatus.Archived, false)]
        [TestCase(WorkflowStatus.Archived, true)]
        public void WhenUpdateNotesThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;
            template.Notes = "Some notes";

            Assert.IsTrue(template.Notes == "Some notes", "Notes should be set correctly");
        }

        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public void WhenUpdateIncludeSubGroupsThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;
            template.IncludeSubGroups = true;

            Assert.IsTrue(template.IncludeSubGroups.Value);
        }

        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public void WhenDeleteStepThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            var createStepId = Guid.NewGuid();
            var delayStepId = Guid.NewGuid();
            template.AddStep(new CreateTaskStep(createStepId, TaskTransition.Immediately, 1));
            template.AddStep(new DelayStep(delayStepId));

            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;
            template.DeleteStep(delayStepId);

            Assert.IsTrue(template.Definition.Steps.First().Id == createStepId);
        }

        [TestCase(WorkflowStatus.Draft, WorkflowStatus.Active, false)]
        [TestCase(WorkflowStatus.Active, WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Active, WorkflowStatus.Archived, false)]
        [TestCase(WorkflowStatus.Active, WorkflowStatus.Archived, true)]
        [TestCase(WorkflowStatus.Active, WorkflowStatus.Draft, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public void WhenUpdateStatusThenVerifyExpectedBehaviour(WorkflowStatus initialStatus, WorkflowStatus destinationStatus, bool inUse)
        {
            template.SetStatus(initialStatus);
            template.CurrentVersion.InUse = inUse;
            template.SetStatus(destinationStatus);

            Assert.IsTrue(template.Status == destinationStatus, "Status should be set correctly");
        }

        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Draft, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public void WhenUpdateRolesThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;

            template.SetRoles(new[] {1, 2, 3});

            var roleIds = string.Join(",", template.Roles.Select(r => r.RoleId));
            Assert.IsTrue(roleIds == "1,2,3", "Roles should be set correctly");
        }

        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Draft, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public void WhenMoveStepUpThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;

            var step = new CreateTaskStep(Guid.NewGuid(), TaskTransition.Immediately, 1);
            var secondStep = new DelayStep(Guid.NewGuid());
            template.AddStep(step);
            template.AddStep(secondStep);
            template.MoveStepUp(secondStep.Id);

            Assert.AreEqual(secondStep, template.Steps.First());
        }

        [TestCase(WorkflowStatus.Draft, false)]
        [TestCase(WorkflowStatus.Draft, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Active, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, false, ExpectedException = typeof (TemplateNotUpdatableException))]
        [TestCase(WorkflowStatus.Archived, true, ExpectedException = typeof (TemplateNotUpdatableException))]
        public void WhenMoveStepDownThenVerifyExpectedBehaviour(WorkflowStatus status, bool inUse)
        {
            template.SetStatus(status);
            template.CurrentVersion.InUse = inUse;

            var step = new CreateTaskStep(Guid.NewGuid(), TaskTransition.Immediately, 1);
            var secondStep = new DelayStep(Guid.NewGuid());
            template.AddStep(step);
            template.AddStep(secondStep);
            template.MoveStepDown(step.Id);

            Assert.AreEqual(secondStep, template.Steps.First());
        }

        [TestCase(WorkflowStatus.Draft, ExpectedResult = "")]
        [TestCase(WorkflowStatus.Archived, ExpectedResult = "TemplateArchived")]
        [TestCase(WorkflowStatus.Active, ExpectedResult = "TemplateMadeActive,TemplateMadeInactive")]
        public string WhenMarkForDeletionThenVerifyTemplateMadeInactiveFired(WorkflowStatus status)
        {
            template.SetStatus(status);
            template.MarkForDeletion();

            return string.Join(",", template.Events.Select(e => e.GetType().Name));
        }

        [TestCase(WorkflowStatus.Draft, WorkflowStatus.Active, ExpectedResult = "TemplateMadeActive")]
        [TestCase(WorkflowStatus.Draft, WorkflowStatus.Archived, ExpectedResult = "TemplateArchived")]
        [TestCase(WorkflowStatus.Active, WorkflowStatus.Archived, ExpectedResult = "TemplateMadeActive,TemplateMadeInactive,TemplateArchived")]
        [TestCase(WorkflowStatus.Active, WorkflowStatus.Draft, ExpectedResult = "TemplateMadeActive,TemplateMadeInactive")]
        [TestCase(WorkflowStatus.Archived, WorkflowStatus.Draft, ExpectedResult = "TemplateArchived")]
        [TestCase(WorkflowStatus.Archived, WorkflowStatus.Active, ExpectedResult = "TemplateArchived,TemplateMadeActive")]
        public string WhenUpdateStatusThenVerifyEventFired(WorkflowStatus startStatus, WorkflowStatus updatedStatus)
        {
            template.SetStatus(startStatus);
            template.SetStatus(updatedStatus);

            return string.Join(",", template.Events.Select(e => e.GetType().Name));
        }
    }
}