using System;
using System.Globalization;
using System.Security.Claims;
using System.ServiceModel;
using System.Threading;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Principal;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.Engine;
using IntelliFlo.Platform.Services.Workflow.Modules;
using IntelliFlo.Platform.Services.Workflow.v1;
using IntelliFlo.Platform.Services.Workflow.v1.Resources;
using Moq;
using NUnit.Framework;
using Constants = IntelliFlo.Platform.Principal.Constants;

namespace IntelliFlo.Platform.Services.Workflow.Tests
{
    [TestFixture]
    public class InstanceResourceTests
    {
        private Mock<IReadOnlyRepository<Instance>> instanceRepository;
        private Mock<IRepository<TemplateDefinition>> templateDefinitionRepository;
        private Mock<IRepository<InstanceHistory>> instanceHistoryRepository;
        private Mock<IDynamicWorkflow> dynamicWorkflow;
        private Mock<IWorkflowHost> workflowHost;
        private IInstanceResource underTest;
        private Instance instance;
        private readonly Guid instanceId = Guid.NewGuid();
        private const int UserId = 123;
        private const int TenantId = 101;


        [SetUp]
        public void SetUp()
        {
            instanceRepository = new Mock<IReadOnlyRepository<Instance>>();
            instanceHistoryRepository = new Mock<IRepository<InstanceHistory>>();
            templateDefinitionRepository = new Mock<IRepository<TemplateDefinition>>();
            dynamicWorkflow = new Mock<IDynamicWorkflow>();
            workflowHost = new Mock<IWorkflowHost>();
            
            instance = new Instance()
            {
                Id = instanceId,
                TenantId = TenantId,
                Template = new TemplateDefinition()
            };

            instanceRepository.Setup(i => i.Get(instanceId)).Returns(instance);

            var identity = new IntelliFloClaimsIdentity("Bob", "Basic");
            identity.AddClaim(new Claim(Constants.ApplicationClaimTypes.UserId, UserId.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(Constants.ApplicationClaimTypes.TenantId, TenantId.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(Constants.ApplicationClaimTypes.Subject, Guid.NewGuid().ToString()));
            Thread.CurrentPrincipal = new IntelliFloClaimsPrincipal(identity);

            underTest = new InstanceResource(instanceRepository.Object, templateDefinitionRepository.Object, instanceHistoryRepository.Object, workflowHost.Object);

            new WorkflowAutoMapperModule().Load();
        }

        [Test]
        [ExpectedException(typeof(InstanceNotFoundException))]
        public void WhenResumeInstanceOnNonExistantInstanceThenExpectException()
        {
            underTest.Resume(Guid.NewGuid(), "TaskCompleted");
        }

        [Test]
        [ExpectedException(typeof(InstancePermissionsException))]
        public void WhenResumeInstanceForDifferentTenantThenExpectException()
        {
            instance.TenantId++;
            underTest.Resume(instanceId, "TaskCompleted");
        }

        [Test]
        public void WhenResumeInstanceThenVerifyCallIsMade()
        {
            underTest.Resume(instanceId, "TaskCompleted");
            workflowHost.Verify(c => c.Resume(It.IsAny<TemplateDefinition>(), It.IsAny<ResumeContext>()), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(InstanceNotFoundException))]
        public void WhenAbortInstanceOnNonExistantInstanceThenExpectException()
        {
            underTest.Abort(Guid.NewGuid());
        }

        [Test]
        [ExpectedException(typeof(InstancePermissionsException))]
        public void WhenAbortInstanceForDifferentTenantThenExpectException()
        {
            instance.TenantId++;
            underTest.Abort(instanceId);
        }

        [Test]
        public void WhenAbortInstanceThenVerifyCallIsMade()
        {
            underTest.Abort(instanceId);
            workflowHost.Verify(c => c.Abort(It.IsAny<TemplateDefinition>(), instanceId), Times.Once());
        }

    }
}
