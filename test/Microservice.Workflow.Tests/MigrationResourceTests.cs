using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IntelliFlo.Platform.Bus;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.NHibernate.Repositories;
using Microservice.Workflow.Collaborators.v1;
using Microservice.Workflow.Domain;
using Microservice.Workflow.Engine;
using Microservice.Workflow.v1;
using Microservice.Workflow.v1.Activities;
using Microservice.Workflow.v1.Resources;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using CreateTaskStep = Microservice.Workflow.Domain.CreateTaskStep;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class MigrationResourceTests
    {
        private IMigrationResource underTest;
        private Instance instance;
        private readonly List<InstanceStep> instanceSteps = new List<InstanceStep>();
        private readonly List<IWorkflowStep> templateSteps = new List<IWorkflowStep>();
        private Mock<IRepository<Instance>> instanceRepository;
        private Mock<IRepository<Template>> templateRepository;
        private Mock<IRepository<TemplateDefinition>> templateDefinitionRepository;
        private Mock<IReadOnlyRepository<InstanceStep>> instanceStepRepository;
        private Mock<IRepository<InstanceHistory>> instanceHistoryRepository;
        private Mock<IHttpClientFactory> clientFactory;
        private Mock<ITrustedClientAuthenticationTokenBuilder> tokenBuilder;
        private Mock<IHttpClient> client;
        private Mock<IWorkflowHost> workflowHost;
        private Mock<IEventDispatcher> eventDispatcher;
        private Mock<IServiceAddressRegistry> addressRegistry;
        private Mock<IServiceEndpoint> serviceEndpoint;
        private const int UserId = 123;
        private const int TenantId = 111;
        private const int TemplateId = 1;
        private Guid stepOneGuid = new Guid("08BF9E40-BD4C-4A2B-9F26-F47AFF3A297B");
        private Guid stepTwoGuid = new Guid("403844C7-934B-475D-8759-BB3675EDC116");

        [SetUp]
        public void SetUp()
        {
            instanceRepository = new Mock<IRepository<Instance>>();
            templateRepository = new Mock<IRepository<Template>>();
            templateDefinitionRepository = new Mock<IRepository<TemplateDefinition>>();
            instanceHistoryRepository = new Mock<IRepository<InstanceHistory>>();
            instanceStepRepository = new Mock<IReadOnlyRepository<InstanceStep>>();

            serviceEndpoint = new Mock<IServiceEndpoint>();
            serviceEndpoint.SetupGet(s => s.BaseAddress).Returns("http://localhost:10111");

            addressRegistry = new Mock<IServiceAddressRegistry>();
            addressRegistry.Setup(a => a.GetServiceEndpoint("workflow")).Returns(() => serviceEndpoint.Object);

            client = new Mock<IHttpClient>();
            clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(c => c.Create(It.IsAny<string>())).Returns(client.Object);

            workflowHost = new Mock<IWorkflowHost>();

            eventDispatcher = new Mock<IEventDispatcher>();

            client.Setup(c => c.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoByUserId, UserId), null)).Returns(() => Task.FromResult(new HttpResponse<Dictionary<string, object>>() {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new Dictionary<string, object> {{IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.Subject, Guid.NewGuid()}}}));

            tokenBuilder = new Mock<ITrustedClientAuthenticationTokenBuilder>();

            var template = new Template("MyTest", TenantId, new TemplateCategory("Test", TenantId), WorkflowRelatedTo.Client, UserId);
            template.AddStep(new CreateTaskStep(stepOneGuid, TaskTransition.Immediately, 123, null));
            template.AddStep(new CreateTaskStep(stepTwoGuid, TaskTransition.Immediately, 123, null));
            instance = new Instance()
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                TenantId = TenantId,
                Template = new TemplateDefinition() { Id = template.Guid, Version = TemplateDefinition.DefaultVersion }
            };

            var templateDefinition = new TemplateDefinition() { Id = template.Guid, Version = TemplateDefinition.DefaultVersion };
            templateDefinitionRepository.Setup(t => t.Get(It.IsAny<Guid>())).Returns(templateDefinition);

            instanceRepository.Setup(i => i.Get(It.IsAny<Guid>())).Returns(instance);
            instanceStepRepository.Setup(i => i.Query()).Returns(instanceSteps.AsQueryable);
            instanceHistoryRepository.Setup(i => i.Query()).Returns(instanceSteps.Select(s => new InstanceHistory(instance.Id, s.StepId, "CreateTask", DateTime.UtcNow)).AsQueryable());

            templateRepository.Setup(t => t.Get(TemplateId)).Returns(template);
            templateRepository.Setup(t => t.Query()).Returns(new[] { template }.AsQueryable());

            underTest = new MigrationResource(templateRepository.Object, templateDefinitionRepository.Object, instanceRepository.Object, instanceStepRepository.Object, addressRegistry.Object, clientFactory.Object, tokenBuilder.Object, workflowHost.Object, eventDispatcher.Object, instanceHistoryRepository.Object);
        }

        [Test]
        public void WhenMigrateDraftTemplateThenSkips()
        {
            var task = underTest.MigrateTemplate(TemplateId);
            var response = task.GetAwaiter().GetResult();
            Assert.AreEqual(MigrationStatus.Skipped.ToString(), response.Status);
        }

        [Test]
        public void WhenMigrateCompletedInstanceThenSkips()
        {
            instance.Status = InstanceStatus.Completed.ToString();
            var task = underTest.MigrateInstance(Guid.NewGuid());
            var response = task.GetAwaiter().GetResult();
            Assert.AreEqual(MigrationStatus.Skipped.ToString(), response.Status);
        }

        [Test]
        public void WhenMigrateInstanceWaitingForTaskThenNewInstanceCreatedCorrectly()
        {
            instance.Status = InstanceStatus.InProgress.ToPrettyString();
            instanceSteps.Add(new InstanceStep()
            {
                Step = StepName.Created.ToString()
            });
            instanceSteps.Add(new InstanceStep()
            {
                InstanceId = instance.Id,
                Step = StepName.CreateTask.ToString(),
                Data = new[]
                {
                    new LogData(), 
                    new LogData() { Detail = new CreateTaskLog(){ TaskId = 123 }},
                    new LogData()
                },
                IsComplete = true
            });
            instanceSteps.Add(new InstanceStep()
            {
                InstanceId = instance.Id,
                Step = StepName.CreateTask.ToString(),
                Data = new[]
                {
                    new LogData(), 
                    new LogData() { Detail = new CreateTaskLog(){ TaskId = 234 }}
                }
            });

            var ctx = MigrateInstance();

            var runToContext = JsonConvert.DeserializeObject<AdditionalContext>(ctx.AdditionalContext);
            Assert.AreEqual(1, runToContext.RunTo.StepIndex);
            Assert.AreEqual(234, runToContext.RunTo.TaskId);
        }

        [Test]
        public void WhenMigrateInstanceWaitingForDelayThenNewInstanceCreatedCorrectly()
        {
            instance.Status = InstanceStatus.InProgress.ToPrettyString();
            instanceSteps.Add(new InstanceStep()
            {
                Step = StepName.Created.ToString()
            });
            instanceSteps.Add(new InstanceStep()
            {
                InstanceId = instance.Id,
                Step = StepName.CreateTask.ToString(),
                Data = new[]
                {
                    new LogData(), 
                    new LogData() { Detail = new CreateTaskLog(){ TaskId = 123 }},
                    new LogData()
                },
                IsComplete = true
            });
            instanceSteps.Add(new InstanceStep()
            {
                InstanceId = instance.Id,
                Step = StepName.Delay.ToString(),
                Data = new[]
                {
                    new LogData(), 
                    new LogData() { Detail = new DelayLog(){ DelayUntil = new DateTime(2015, 7, 7, 14, 22, 0, DateTimeKind.Utc)}}
                }
            });

            var ctx = MigrateInstance();

            var runToContext = JsonConvert.DeserializeObject<AdditionalContext>(ctx.AdditionalContext);
            Assert.AreEqual(1, runToContext.RunTo.StepIndex);
            Assert.AreEqual(new DateTime(2015, 7, 7, 14, 22, 0, DateTimeKind.Utc), runToContext.RunTo.DelayTime);
        }

        [Test]
        public void WhenMigrateInstanceThenCorrectStepIdIsSet()
        {
            instance.Status = InstanceStatus.InProgress.ToPrettyString();
            instance.Version = TemplateDefinition.DefaultVersion;
            instanceSteps.Add(new InstanceStep()
            {
                Step = StepName.Created.ToString()
            });
            instanceSteps.Add(new InstanceStep()
            {
                Id = Guid.NewGuid().ToString(),
                StepId = Guid.NewGuid(),
                InstanceId = instance.Id,
                Step = StepName.CreateTask.ToString(),
                Data = new[]
                {
                    new LogData(),
                    new LogData() { Detail = new CreateTaskLog(){ TaskId = 123 }},
                    new LogData()
                },
                IsComplete = true
            });
            instanceSteps.Add(new InstanceStep()
            {
                Id = Guid.NewGuid().ToString(),
                StepId = Guid.NewGuid(),
                InstanceId = instance.Id,
                Step = StepName.Delay.ToString(),
                Data = new[]
                {
                    new LogData(),
                    new LogData() { Detail = new DelayLog(){ DelayUntil = new DateTime(2015, 7, 7, 14, 22, 0, DateTimeKind.Utc)}}
                }
            });

            MigrateInstance();

            instanceHistoryRepository.Verify(i => i.Save(It.Is<InstanceHistory>(h => h.StepId == stepOneGuid)), Times.Exactly(1));
            instanceHistoryRepository.Verify(i => i.Save(It.Is<InstanceHistory>(h => h.StepId == stepTwoGuid)), Times.Exactly(1));
        }

        [Test]
        public void WhenMigrateInstanceAndInstanceAlreadyProgressedThenCorrectStepIdIsSet()
        {
            instance.Status = InstanceStatus.InProgress.ToPrettyString();
            instance.Version = TemplateDefinition.DefaultVersion;
            instanceSteps.Add(new InstanceStep()
            {
                Step = StepName.Created.ToString()
            });
            instanceSteps.Add(new InstanceStep()
            {
                Id = Guid.NewGuid().ToString(),
                StepId =  Guid.NewGuid(),
                InstanceId = instance.Id,
                Step = StepName.CreateTask.ToString(),
                Data = new[]
                {
                    new LogData(),
                    new LogData() { Detail = new CreateTaskLog(){ TaskId = 123 }},
                    new LogData()
                },
                IsComplete = false
            });
            instanceSteps.Add(new InstanceStep()
            {
                Id = stepOneGuid.ToString(),
                StepId = stepOneGuid,
                InstanceId = instance.Id,
                Step = StepName.CreateTask.ToString(),
                IsComplete = true
            });
            instanceSteps.Add(new InstanceStep()
            {
                Id = stepTwoGuid.ToString(),
                StepId = stepTwoGuid,
                InstanceId = instance.Id,
                Step = StepName.Delay.ToString(),
                Data = new[]
                {
                    new LogData(),
                    new LogData() { Detail = new DelayLog(){ DelayUntil = new DateTime(2015, 7, 7, 14, 22, 0, DateTimeKind.Utc)}}
                }
            });
            
            MigrateInstance();

            instanceHistoryRepository.Verify(i => i.Save(It.Is<InstanceHistory>(h => h.StepId == stepOneGuid)), Times.Exactly(1));
        }
        
        private WorkflowContext MigrateInstance()
        {
            WorkflowContext context = null;
            workflowHost.Setup(c => c.Create(It.IsAny<TemplateDefinition>(), It.IsAny<WorkflowContext>())).Callback<TemplateDefinition, WorkflowContext>((t, c) => context = c).Returns(Guid.NewGuid);

            var task = underTest.MigrateInstance(instance.Id);
            task.Wait();

            return context;
        }


    }
}
