using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Autofac;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Principal;
using log4net.Config;
using Microservice.Workflow.Domain;
using Microservice.Workflow.Engine;
using Microservice.Workflow.Modules;
using Microservice.Workflow.v1;
using Microservice.Workflow.v1.Activities;
using Microservice.Workflow.v1.Contracts;
using Microservice.Workflow.v1.Resources;
using Moq;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class TemplateResourceTests
    {
        private ITemplateResource underTest;
        private Mock<IRepository<Template>> templateRepository;
        private Mock<IRepository<TemplateCategory>> templateCategoryRepository;
        private Mock<IRepository<TemplateDefinition>> templateDefinitionRepository;
        private Mock<IRepository<TemplateRegistration>> templateRegistrationRepository;
        private Mock<ITrustedClientAuthenticationTokenBuilder> tokenBuilder;
        private Mock<IHttpClientFactory> clientFactory;
        private Mock<IHttpClient> client;
        private Mock<IEventDispatcher> eventDispatcher;
        private Mock<IWorkflowHost> workflowHost;
        private Template template;
        private TemplateCategory category;
        private TemplateCategory altCategory;
        private const int TenantId = 1123;
        private const int OwnerUserId = 343;
        private const int TemplateId = 101;
        private const int RoleId = 993;
        private const int GroupId = 932;
        private const int ParentGroupId = 34;

        [SetUp]
        public void SetUp()
        {
            XmlConfigurator.Configure();

            category = new TemplateCategory("Test", TenantId) { Id = 1 };
            altCategory = new TemplateCategory("Alt", TenantId) { Id = 2};

            template = new Template("Test", TenantId, category, WorkflowRelatedTo.Client, OwnerUserId) { Id = TemplateId };

            templateRepository = new Mock<IRepository<Template>>();
            templateRepository.Setup(t => t.Get(It.IsAny<int>())).Returns(template);

            templateCategoryRepository = new Mock<IRepository<TemplateCategory>>();
            templateCategoryRepository.Setup(t => t.Load(1)).Returns(category);
            templateCategoryRepository.Setup(t => t.Load(2)).Returns(altCategory);

            templateRegistrationRepository = new Mock<IRepository<TemplateRegistration>>();

            tokenBuilder = new Mock<ITrustedClientAuthenticationTokenBuilder>();
            templateDefinitionRepository = new Mock<IRepository<TemplateDefinition>>();
            
            clientFactory = new Mock<IHttpClientFactory>();
            client = new Mock<IHttpClient>();
            eventDispatcher = new Mock<IEventDispatcher>();

            workflowHost = new Mock<IWorkflowHost>();

            clientFactory.Setup(c => c.Create(It.IsAny<string>())).Returns(client.Object);
            var serviceFactory = new WorkflowServiceFactory(new DayDelayPeriod(), clientFactory.Object);

            underTest = new TemplateResource(templateRepository.Object, templateCategoryRepository.Object, templateRegistrationRepository.Object, tokenBuilder.Object, clientFactory.Object, templateDefinitionRepository.Object, serviceFactory, eventDispatcher.Object, workflowHost.Object);

            var builder = new ContainerBuilder();
            builder.RegisterInstance(underTest).AsImplementedInterfaces();
            var container = builder.Build();

            Microservice.Workflow.IoC.Initialize(container);

            var identity = new IntelliFloClaimsIdentity("Bob", "Basic");
            identity.AddClaim(new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.UserId, OwnerUserId.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.TenantId, TenantId.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.Subject, Guid.NewGuid().ToString()));
            identity.AddClaim(new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.RoleId, RoleId.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.GroupLineage, string.Join(",", new[] { ParentGroupId, GroupId })));
            Thread.CurrentPrincipal = new IntelliFloClaimsPrincipal(identity);

            new WorkflowAutoMapperModule().Load();

        }

        [TestCase(WorkflowStatus.Archived, ExpectedException = typeof(TemplateNotActiveException))]
        [TestCase(WorkflowStatus.Draft, ExpectedException = typeof(TemplateNotActiveException))]
        public void WhenCreateNonActiveInstanceThenExpectException(WorkflowStatus status)
        {
            template.SetStatus(status);
            underTest.CreateInstance(TemplateId, new CreateInstanceRequest());
        }
        
        [Test]
        [ExpectedException(typeof(TemplatePermissionsException))]
        public void WhenCreateInstanceAndUserRoleNotAssignedToTemplateThenExpectException()
        {
            template.SetStatus(WorkflowStatus.Active);
            underTest.CreateInstance(TemplateId, new CreateInstanceRequest());
        }

        [TestCase(GroupId, false)]
        [TestCase(GroupId, true)]
        [TestCase(ParentGroupId, true)]
        [TestCase(ParentGroupId, false, ExpectedException = typeof(TemplatePermissionsException))]
        [TestCase(ParentGroupId + 1, true, ExpectedException = typeof(TemplatePermissionsException))]
        public void WhenCreateInstanceAndUserGroupNotAssignedToTemplateThenExpectException(int templateGroupId, bool includeSubGroups)
        {
            template.SetRoles(new[] { RoleId });
            template.ApplicableToGroupId = templateGroupId;
            template.IncludeSubGroups = includeSubGroups;
            template.SetStatus(WorkflowStatus.Active);
            underTest.CreateInstance(TemplateId, new CreateInstanceRequest());
        }

        [Test]
        public void WhenCreateInstanceThenCreatedSuccessfully()
        {
            template.SetRoles(new[] { RoleId });
            template.SetStatus(WorkflowStatus.Active);
            underTest.CreateInstance(TemplateId, new CreateInstanceRequest());
        }

        [Test]
        [ExpectedException(typeof(TemplateNotUniqueException))]
        public void WhenCreateTemplateThenValidateNameIsUniqueForTenant()
        {
            var templates = new List<Template>()
            {
                template,
                new Template("Test1", TenantId, category, WorkflowRelatedTo.Client, OwnerUserId) { Id = TemplateId + 1 },
                new Template("Test1", TenantId + 1, category, WorkflowRelatedTo.Client, OwnerUserId + 1) { Id = TemplateId + 2 }
            };
            templateRepository.Setup(t => t.Query()).Returns(templates.AsQueryable);

            underTest.Post(new CreateTemplateRequest()
            {
                Name = "Test1"
            });
        }

        [Test]
        public void WhenCloneTemplateThenSuccessful()
        {
            var clone = underTest.Clone(TemplateId, new CloneTemplateRequest() {Name = "Clone1"});
            Assert.AreEqual("Clone1", clone.Name);
            Assert.AreEqual(template.Category.Id, clone.Category.TemplateCategoryId);
            Assert.AreEqual(template.RelatedTo.ToString(), clone.RelatedTo);
            Assert.AreEqual(template.ApplicableToGroupId, clone.ApplicableToGroupId);
            Assert.AreEqual(template.IncludeSubGroups, clone.IncludeSubGroups);
        }

        [Test]
        [ExpectedException(typeof(TemplateNotUniqueException))]
        public void WhenPatchNameThenValidateUniqueness()
        {
            var templates = new List<Template>()
            {
                template,
                new Template("Test1", TenantId, category, WorkflowRelatedTo.Client, OwnerUserId) { Id = TemplateId + 1 },
                new Template("Test1", TenantId + 1, category, WorkflowRelatedTo.Client, OwnerUserId + 1) { Id = TemplateId + 2 }
            };
            templateRepository.Setup(t => t.Query()).Returns(templates.AsQueryable);

            underTest.Patch(TemplateId, new TemplatePatchRequest() {Name = "Test1"});
        }

        [Test]
        public void WhenPatchTemplateThenUpdatedSuccessfully()
        {
            var patched = underTest.Patch(TemplateId, new TemplatePatchRequest()
            {
                Name = "Patch",
                ApplicableToGroup = true,
                ApplicableToGroupId = 101,
                IncludeSubGroups = true,
                Notes = "Patched",
                OwnerUserId = OwnerUserId + 1,
                Status = WorkflowStatus.Active.ToString(),
                TemplateCategoryId = altCategory.Id
            });

            Assert.AreEqual("Patch", patched.Name);
            Assert.AreEqual(101, patched.ApplicableToGroupId);
            Assert.IsTrue(patched.IncludeSubGroups.Value);
            Assert.AreEqual("Patched", patched.Notes);
            Assert.AreEqual(OwnerUserId + 1, patched.OwnerUserId);
            Assert.AreEqual(WorkflowStatus.Active.ToString(), patched.Status);
            Assert.AreEqual(altCategory.Id, patched.Category.TemplateCategoryId);
        }
    }
}
