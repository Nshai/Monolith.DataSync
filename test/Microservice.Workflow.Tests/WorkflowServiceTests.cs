using System;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Activities;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xaml;
using System.Xml.Linq;
using Autofac;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Impl;
using IntelliFlo.Platform.Identity;
using log4net.Config;
using Microservice.Workflow.Collaborators.v1;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1;
using Microservice.Workflow.v1.Activities;
using Microservice.Workflow.v1.Resources;
using Moq;
using Newtonsoft.Json;
using NHibernate;
using NUnit.Framework;
using Constants = IntelliFlo.Platform.Principal.Constants;
using CreateTaskStep = Microservice.Workflow.Domain.CreateTaskStep;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class WorkflowServiceTests
    {
        private Mock<IServiceHttpClientFactory> serviceClientFactory;
        private Mock<IServiceHttpClient> serviceClient;
        private Mock<ITrustedClientAuthenticationScheme> trustedClientAuthenticationScheme;
        private Mock<IServiceAddressRegistry> addressRegistry;
        private IWorkflowServiceFactory serviceFactory;
        private Mock<ISessionFactory> sessionFactory;
        private Mock<ISession> session;
        private Mock<ITransaction> transaction;
        private Template clientTemplate;
        private Template leadTemplate;
        private Template adviserTemplate;
        private Template serviceCaseTemplate;
        private Template planTemplate;
        private const string HostAddress = "net.pipe://localhost";

        private const int TenantId = 101;
        private const int EntityId = 123;
        private const int UserId = 1;
        private const int OwnerPartyId = 324;
        private const int OwnerUserId = 1233;

        [SetUp]
        public void SetUp()
        {
            XmlConfigurator.Configure();

            serviceClientFactory = new Mock<IServiceHttpClientFactory>();
            serviceClient = new Mock<IServiceHttpClient>();
            trustedClientAuthenticationScheme = new Mock<ITrustedClientAuthenticationScheme>();
            addressRegistry = new Mock<IServiceAddressRegistry>();

            serviceClient.Setup(c => c.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoByUserId, OwnerUserId), null)).Returns(Task.FromResult(new HttpResponse<Dictionary<string, object>> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new Dictionary<string, object> {{Constants.ApplicationClaimTypes.PartyId, OwnerPartyId}}}));
            serviceClientFactory.Setup(c => c.Create(It.IsAny<string>())).Returns(serviceClient.Object);
            var entityTaskFactory = new EntityTaskBuilderFactory(serviceClientFactory.Object);

            addressRegistry.Setup(a => a.GetServiceEndpoint("workflow")).Returns(new ServiceEndpointElement {Name = "workflow", BaseAddress = "http://localhost:10083/workflow/"});

            serviceFactory = new WorkflowServiceFactory(new DayDelayPeriod(), serviceClientFactory.Object);

            var claims = new List<Claim>
            {
                new Claim(Constants.ApplicationClaimTypes.UserId, UserId.ToString(CultureInfo.InvariantCulture)),
                new Claim(Constants.ApplicationClaimTypes.TenantId, TenantId.ToString(CultureInfo.InvariantCulture)),
                new Claim(Constants.ApplicationClaimTypes.Subject, Guid.NewGuid().ToString())
            };

            trustedClientAuthenticationScheme.Setup(c => c.Validate(It.IsAny<string>())).Returns(Task.FromResult(claims.AsEnumerable()));
            var category = new TemplateCategory("Test", TenantId);
            clientTemplate = new Template("Test", TenantId, category, WorkflowRelatedTo.Client, OwnerUserId);
            leadTemplate = new Template("Test", TenantId, category, WorkflowRelatedTo.Lead, OwnerUserId);
            adviserTemplate = new Template("Test", TenantId, category, WorkflowRelatedTo.Adviser, OwnerUserId);
            serviceCaseTemplate = new Template("Test", TenantId, category, WorkflowRelatedTo.ServiceCase, OwnerUserId);
            planTemplate = new Template("Test", TenantId, category, WorkflowRelatedTo.Plan, OwnerUserId);

            sessionFactory = new Mock<ISessionFactory>();
            
            session = new Mock<ISession>();
            transaction = new Mock<ITransaction>();
            sessionFactory.Setup(s => s.OpenSession()).Returns(session.Object);
            session.Setup(s => s.BeginTransaction()).Returns(transaction.Object);

            var builder = new ContainerBuilder();
            builder.RegisterInstance(trustedClientAuthenticationScheme.Object).As<ITrustedClientAuthenticationScheme>();
            builder.RegisterInstance(serviceClientFactory.Object).As<IServiceHttpClientFactory>();
            builder.RegisterInstance(entityTaskFactory).As<IEntityTaskBuilderFactory>();
            builder.RegisterInstance(addressRegistry.Object).As<IServiceAddressRegistry>();
            builder.RegisterInstance(sessionFactory.Object).As<ISessionFactory>();
            Microservice.Workflow.IoC.Initialize(Microservice.Workflow.Engine.Constants.ContainerId, builder.Build());
        }

        [Test]
        public void WhenCreateInstanceThenInstanceIsRegistered()
        {
            using (CreateHost(clientTemplate))
            {
                var instanceId = CreateInstance(new WorkflowContext {BearerToken = "123", EntityType = EntityType.Client.ToString(), EntityId = EntityId});
                session.Verify(i => i.Save(It.IsAny<Instance>()));
                Assert.IsTrue(instanceId != Guid.Empty);
            }
        }

        [Test]
        public void WhenCreateInstanceAsyncThenCreatedSuccessfully()
        {
            using (CreateHost(clientTemplate))
            {
                CreateInstance(new WorkflowContext {BearerToken = "123", EntityType = EntityType.Client.ToString(), EntityId = EntityId}, true);
            }
        }

        [Test]
        public void WhenCreateInstanceWithInvalidContextThenExpectException()
        {
            try
            {
                using (CreateHost(clientTemplate))
                {
                    CreateInstance(new WorkflowContext {BearerToken = "123", EntityType = EntityType.Plan.ToString(), EntityId = EntityId});
                }
            }
            catch (FaultException ex)
            {
                Assert.AreEqual(FaultCodes.InvalidContext, ex.Code.Name);
            }
        }

        [Test]
        public void WhenCreateInstanceWithCreateTaskStepThenTaskIsCreatedSuccessfully()
        {
            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.User, assignedToPartyId: OwnerPartyId);
            ExecuteCreateTaskWorkflow(clientTemplate, new[] {createTask});
            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Once());
        }

        [Test]
        public void WhenExecuteCreateTaskStepWithOwnerPartyThenTaskCreatedSuccessfully()
        {
            const int partyId = 1234;
            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.User, assignedToPartyId: partyId);
            var request = ExecuteCreateTaskWorkflow(clientTemplate, new[] {createTask});

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Once());

            Assert.AreEqual(EntityId, request.RelatedPartyId);
            Assert.AreEqual(partyId, request.AssignedToPartyId);
        }

        [Test]
        public void WhenExecuteCreateTaskStepWithOwnerRoleThenTaskCreatedSuccessfully()
        {
            const int roleId = 3742;
            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.Role, assignedToRoleId: roleId);
            var request = ExecuteCreateTaskWorkflow(clientTemplate, new[] {createTask});

            Assert.AreEqual(EntityId, request.RelatedPartyId);
            Assert.AreEqual(roleId, request.AssignedToRoleId);
        }

        [Test]
        public void WhenExecuteCreateTaskStepWithOwnerContextRoleThenTaskCreatedSuccessfully()
        {
            const int adviserId = 9393;
            serviceClient.Setup(c => c.Get<ClientDocument>(string.Format(Uris.Crm.GetClient, EntityId), null)).Returns(() => Task.FromResult(new HttpResponse<ClientDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new ClientDocument {CurrentAdviserPartyId = adviserId}}));

            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.ContextRole, assignedToRoleContext: RoleContextType.ServicingAdviser);
            var request = ExecuteCreateTaskWorkflow(clientTemplate, new[] {createTask});

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Once());

            Assert.AreEqual(EntityId, request.RelatedPartyId);
            Assert.AreEqual(adviserId, request.AssignedToPartyId);
        }

        [Test]
        public void WhenExecuteCreateTaskStepForLeadWithOwnerContextRoleThenTaskCreatedSuccessfully()
        {
            const int adviserId = 9393;
            serviceClient.Setup(c => c.Get<LeadDocument>(string.Format(Uris.Crm.GetLead, EntityId), null)).Returns(() => Task.FromResult(new HttpResponse<LeadDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new LeadDocument {CurrentAdviserPartyId = adviserId}}));

            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.ContextRole, assignedToRoleContext: RoleContextType.Adviser);
            var request = ExecuteCreateTaskWorkflow(leadTemplate, new[] {createTask}, c => c.EntityType = EntityType.Lead.ToString());

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Once());

            Assert.AreEqual(EntityId, request.RelatedPartyId);
            Assert.AreEqual(adviserId, request.AssignedToPartyId);
        }

        [Test]
        public void WhenExecuteCreateTaskStepForLeadConvertedToClientWithOwnerContextRoleThenTaskCreatedSuccessfully()
        {
            const int adviserId = 9393;
            serviceClient.Setup(c => c.Get<LeadDocument>(string.Format(Uris.Crm.GetLead, EntityId), null)).Returns(() => Task.FromResult(new HttpResponse<LeadDocument> {Raw = new HttpResponseMessage(HttpStatusCode.NotFound)}));
            serviceClient.Setup(c => c.Get<ClientDocument>(string.Format(Uris.Crm.GetClient, EntityId), null)).Returns(() => Task.FromResult(new HttpResponse<ClientDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new ClientDocument {CurrentAdviserPartyId = adviserId}}));

            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.ContextRole, assignedToRoleContext: RoleContextType.Adviser);
            var request = ExecuteCreateTaskWorkflow(leadTemplate, new[] {createTask}, c => c.EntityType = EntityType.Lead.ToString());

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Once());

            Assert.AreEqual(EntityId, request.RelatedPartyId);
            Assert.AreEqual(adviserId, request.AssignedToPartyId);
        }

        [Test]
        public void WhenExecuteCreateTaskStepForServiceCaseWithOwnerContextRoleThenTaskCreatedSuccessfully()
        {
            const int clientId = 111;
            const int adviserId = 9393;
            serviceClient.Setup(c => c.Get<ServiceCaseDocument>(string.Format(Uris.Crm.GetServiceCase, clientId, EntityId), null)).Returns(() => Task.FromResult(new HttpResponse<ServiceCaseDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new ServiceCaseDocument {ServicingAdviserId = adviserId}}));

            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.ContextRole, assignedToRoleContext: RoleContextType.Adviser);
            var request = ExecuteCreateTaskWorkflow(serviceCaseTemplate, new[] {createTask}, c =>
            {
                c.EntityType = EntityType.ServiceCase.ToString();
                c.ClientId = clientId;
            });

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Once());

            Assert.AreEqual(clientId, request.RelatedPartyId);
            Assert.AreEqual(adviserId, request.AssignedToPartyId);
        }

        [Test]
        public void WhenExecuteCreateTaskStepForPlanWithOwnerContextRoleThenTaskCreatedSuccessfully()
        {
            const int clientId = 111;
            const int adviserId = 9393;
            serviceClient.Setup(c => c.Get<PlanDocument>(string.Format(Uris.Portfolio.GetPlan, clientId, EntityId), null)).Returns(() => Task.FromResult(new HttpResponse<PlanDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new PlanDocument {SellingAdviserPartyId = adviserId, Owners = new[] {new PlanDocument.PlanOwner {ClientId = clientId, PlanOwnerId = clientId}}}}));

            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.ContextRole, assignedToRoleContext: RoleContextType.SellingAdviser);
            var request = ExecuteCreateTaskWorkflow(planTemplate, new[] {createTask}, c =>
            {
                c.EntityType = EntityType.Plan.ToString();
                c.ClientId = clientId;
            });

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Once());

            Assert.AreEqual(clientId, request.RelatedPartyId);
            Assert.AreEqual(adviserId, request.AssignedToPartyId);
        }

        [Test]
        public void WhenExecuteCreateTaskStepForAdviserWithOwnerContextRoleThenTaskCreatedSuccessfully()
        {
            const int tAndCCoachPartyId = 345;
            serviceClient.Setup(c => c.Get<AdviserDocument>(string.Format(Uris.Crm.GetAdviser, EntityId), null)).Returns(Task.FromResult(new HttpResponse<AdviserDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new AdviserDocument {TnCCoachPartyId = tAndCCoachPartyId}}));

            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.ContextRole, assignedToRoleContext: RoleContextType.TandCCoach);
            var request = ExecuteCreateTaskWorkflow(adviserTemplate, new[] {createTask}, c => { c.EntityType = EntityType.Adviser.ToString(); });

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Once());

            Assert.AreEqual(tAndCCoachPartyId, request.AssignedToPartyId);
        }

        [Test]
        public void WhenExecuteCreateTaskStepWithSkipToThisStepThenTaskIsNotCreatedButSubscriptionAdded()
        {
            const int clientId = 111;
            const int adviserId = 9393;
            serviceClient.Setup(c => c.Get<PlanDocument>(string.Format(Uris.Portfolio.GetPlan, clientId, EntityId), null)).Returns(() => Task.FromResult(new HttpResponse<PlanDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new PlanDocument {SellingAdviserPartyId = adviserId, Owners = new[] {new PlanDocument.PlanOwner {ClientId = clientId, PlanOwnerId = clientId}}}}));
            serviceClient.Setup(c => c.Post<EventSubscriptionDocument, SubscribeRequest>(Uris.EventManagement.Post, It.IsAny<SubscribeRequest>())).Returns(Task.FromResult(new HttpResponse<EventSubscriptionDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new EventSubscriptionDocument()}));

            var additionalContext = JsonConvert.SerializeObject(new AdditionalContext {RunTo = new RunToAdditionalContext {StepIndex = 0, TaskId = 776}});

            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.ContextRole, assignedToRoleContext: RoleContextType.SellingAdviser);
            var request = ExecuteCreateTaskWorkflow(planTemplate, new[] {createTask}, c =>
            {
                c.EntityType = EntityType.Plan.ToString();
                c.ClientId = clientId;
                c.AdditionalContext = additionalContext;
            });

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Never());
            serviceClient.Verify(c => c.Post<EventSubscriptionDocument, SubscribeRequest>(Uris.EventManagement.Post, It.IsAny<SubscribeRequest>()), Times.Exactly(2));

            Assert.IsNull(request);
        }

        [Test]
        public void WhenExecuteCreateTaskStepWithSkipToNextStepThenTaskIsNotCreated()
        {
            const int clientId = 111;
            const int adviserId = 9393;
            serviceClient.Setup(c => c.Get<PlanDocument>(string.Format(Uris.Portfolio.GetPlan, clientId, EntityId), null)).Returns(() => Task.FromResult(new HttpResponse<PlanDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new PlanDocument {SellingAdviserPartyId = adviserId, Owners = new[] {new PlanDocument.PlanOwner {ClientId = clientId, PlanOwnerId = clientId}}}}));

            var additionalContext = JsonConvert.SerializeObject(new AdditionalContext {RunTo = new RunToAdditionalContext {StepIndex = 1, TaskId = 776}});

            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.ContextRole, assignedToRoleContext: RoleContextType.SellingAdviser);
            var request = ExecuteCreateTaskWorkflow(planTemplate, new[] {createTask}, c =>
            {
                c.EntityType = EntityType.Plan.ToString();
                c.ClientId = clientId;
                c.AdditionalContext = additionalContext;
            });

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Never());
            serviceClient.Verify(c => c.Post<EventSubscriptionDocument, SubscribeRequest>(Uris.EventManagement.Post, It.IsAny<SubscribeRequest>()), Times.Never());

            Assert.IsNull(request);
        }

        [Test]
        public void WhenExecuteCreateTaskStepWithWithForCompletionThenResumesSuccessfully()
        {
            var createTaskSteps = new[]
            {
                new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.User, assignedToPartyId: OwnerPartyId),
                new CreateTaskStep(Guid.NewGuid(), TaskTransition.Immediately, 222, TaskAssignee.User, assignedToPartyId: OwnerPartyId)
            };

            serviceClient
                .Setup(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()))
                .Returns(Task.FromResult(new HttpResponse<TaskDocument> { Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new TaskDocument { TaskId = 999 } }));

            serviceClient
                .Setup(c => c.Post<EventSubscriptionDocument, SubscribeRequest>(Uris.EventManagement.Post, It.IsAny<SubscribeRequest>()))
                .Returns(Task.FromResult(new HttpResponse<EventSubscriptionDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new EventSubscriptionDocument()}));

            foreach (var step in createTaskSteps)
            {
                clientTemplate.AddStep(step);
            }

            using (CreateHost(clientTemplate))
            {
                var instanceId = CreateInstance();

                serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Once());
                serviceClient.Verify(c => c.Post<EventSubscriptionDocument, SubscribeRequest>(Uris.EventManagement.Post, It.IsAny<SubscribeRequest>()), Times.Exactly(2));
                
                ResumeInstance(new ResumeContext()
                {
                    InstanceId = instanceId,
                    BookmarkName = "TaskCompleted:999"
                });
            }

            serviceClient.Verify(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()), Times.Exactly(2));
        }

        [Test]
        public void WhenExecuteCreateTaskStepWithDueDelayForBusinessDaysThenVerifyHolidayApiCalledCorrectly()
        {
            var uri = "";
            serviceClient.Setup(c => c.Get<IEnumerable<HolidayDocument>>(It.IsAny<string>(), It.IsAny<long?>())).Callback<string, long?>((u, e) => uri = u).Returns(Task.FromResult(new HttpResponse<IEnumerable<HolidayDocument>>()));

            var createTask = new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, 111, TaskAssignee.User, 2, true, OwnerPartyId);
            ExecuteCreateTaskWorkflow(clientTemplate, new[] { createTask });

            var holidayUriRegex = new Regex(@".*\?from=[\d\-\w:]*&to=[\d\-\w:]*");

            Assert.IsTrue(holidayUriRegex.IsMatch(uri), "Holiday request was not properly formatted");
        }

        private Guid CreateInstance(Action<WorkflowContext> modifyContext = null)
        {
            var context = new WorkflowContext { BearerToken = "123", EntityType = EntityType.Client.ToString(), EntityId = EntityId };
            if (modifyContext != null)
                modifyContext(context);
            
            return CreateInstance(context);
        }

        private CreateTaskRequest ExecuteCreateTaskWorkflow(Template testTemplate, IEnumerable<CreateTaskStep> createTaskSteps, Action<WorkflowContext> modifyContext = null)
        {
            CreateTaskRequest request = null;
            serviceClient
                .Setup(c => c.Post<TaskDocument, CreateTaskRequest>(Uris.Crm.CreateTask, It.IsAny<CreateTaskRequest>()))
                .Callback<string, CreateTaskRequest>((u, r) => { request = r; })
                .Returns(Task.FromResult(new HttpResponse<TaskDocument> {Raw = new HttpResponseMessage(HttpStatusCode.OK), Resource = new TaskDocument {TaskId = 999}}));

            foreach (var step in createTaskSteps)
            {
                testTemplate.AddStep(step);
            }

            using(CreateHost(testTemplate))
            {
                CreateInstance(modifyContext);
            }

            return request;
        }

        private IDisposable CreateHost(Template testTemplate)
        {
            var xaml = serviceFactory.Build(testTemplate);
            using (var reader = new StringReader(xaml))
            using (var xamlReader = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(reader)))
            {
                var workflow = XamlServices.Load(xamlReader) as WorkflowService;
                var host = new WorkflowServiceHost(workflow, new Uri(HostAddress));
                host.AddServiceEndpoint(XName.Get("IDynamicWorkflow", "http://intelliflo.com/dynamicworkflow/2014/06"), new NetNamedPipeBinding(NetNamedPipeSecurityMode.None), "net.pipe://localhost");
                host.Open();
                return host;
            }
        }

        public Guid CreateInstance(WorkflowContext ctx, bool async = false)
        {
            using (var client = new DynamicWorkflowClient(new NetNamedPipeBinding(NetNamedPipeSecurityMode.None), new EndpointAddress(HostAddress)))
            {
                if (!async) return client.Create(ctx);
                client.CreateAsync(ctx);
                return Guid.Empty;
            }
        }

        private static void ResumeInstance(ResumeContext ctx)
        {
            using (var client = new DynamicWorkflowClient(new NetNamedPipeBinding(NetNamedPipeSecurityMode.None), new EndpointAddress(HostAddress)))
            {
                client.Resume(ctx);
            }
        }
    }
}