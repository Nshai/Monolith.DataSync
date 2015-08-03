using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Services.Workflow.Collaborators.v1;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1;
using IntelliFlo.Platform.Services.Workflow.v1.Activities;
using IntelliFlo.Platform.Services.Workflow.v1.Resources;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IntelliFlo.Platform.Services.Workflow.Tests
{
    [TestFixture]
    public class MigrationResourceTests
    {
        private IMigrationResource underTest;
        private Instance instance;
        private readonly List<InstanceStep> instanceSteps = new List<InstanceStep>();
        private Mock<IRepository<Instance>> instanceRepository;
        private Mock<IRepository<Template>> templateRepository;
        private Mock<IReadOnlyRepository<InstanceStep>> instanceStepRepository;
        private Mock<IServiceHttpClientFactory> clientFactory;
        private Mock<ITrustedClientAuthenticationTokenBuilder> tokenBuilder;
        private Mock<IServiceHttpClient> client;
        private readonly Mock<IWorkflowClientFactory> workflowClientFactory = new Mock<IWorkflowClientFactory>();
        private Mock<IDynamicWorkflow> workflowClient;
        private Mock<IEventDispatcher> eventDispatcher;
        private const int UserId = 123;
        private const int TenantId = 111;

        [SetUp]
        public void SetUp()
        {
            instanceRepository = new Mock<IRepository<Instance>>();
            templateRepository = new Mock<IRepository<Template>>();
            instanceStepRepository = new Mock<IReadOnlyRepository<InstanceStep>>();
            var workflowConfiguration = new WorkflowConfiguration() { EndpointAddress = "http://localhost:10084/xaml/" };
            client = new Mock<IServiceHttpClient>();
            clientFactory = new Mock<IServiceHttpClientFactory>();
            clientFactory.Setup(c => c.Create(It.IsAny<string>())).Returns(client.Object);

            workflowClient = new Mock<IDynamicWorkflow>();
            workflowClientFactory.Setup(c => c.GetDynamicClient(It.IsAny<string>(), It.IsAny<EndpointAddress>())).Returns(workflowClient.Object);

            eventDispatcher = new Mock<IEventDispatcher>();

            client.Setup(c => c.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoByUserId, UserId), null)).Returns(() => Task.FromResult(new HttpResponse<Dictionary<string, object>>() {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new Dictionary<string, object> {{Principal.Constants.ApplicationClaimTypes.Subject, Guid.NewGuid()}}}));

            tokenBuilder = new Mock<ITrustedClientAuthenticationTokenBuilder>();

            instance = new Instance()
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                TenantId = TenantId,
                Template = new TemplateDefinition() {  Version = 0}
            };

            instanceRepository.Setup(i => i.Get(It.IsAny<Guid>())).Returns(instance);
            instanceStepRepository.Setup(i => i.Query()).Returns(instanceSteps.AsQueryable);

            underTest = new MigrationResource(templateRepository.Object, instanceRepository.Object, instanceStepRepository.Object, workflowConfiguration, clientFactory.Object, tokenBuilder.Object, workflowClientFactory.Object, eventDispatcher.Object);
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
            instance.Status = InstanceStatus.Processing.ToString();
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
            instance.Status = InstanceStatus.Processing.ToString();
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

        private WorkflowContext MigrateInstance()
        {
            WorkflowContext context = null;
            workflowClient.Setup(c => c.Create(It.IsAny<WorkflowContext>())).Callback<WorkflowContext>(c => context = c).Returns(Guid.NewGuid);

            var task = underTest.MigrateInstance(instance.Id);
            task.Wait();

            return context;
        }


    }
}
