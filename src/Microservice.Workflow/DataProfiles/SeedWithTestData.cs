using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IntelliFlo.Platform.Database;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.NHibernate;
using IntelliFlo.Platform.Principal;
using Microservice.Workflow.Collaborators.v1;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Activities;
using Moq;
using NHibernate.Linq;
using NUnit.Framework;
using Constants = IntelliFlo.Platform.Principal.Constants;
using CreateTaskStep = Microservice.Workflow.Domain.CreateTaskStep;

namespace Microservice.Workflow.DataProfiles
{
    public class SeedWithTestData : TaskBase<SeedWithTestData>
    {
        private readonly IReadWriteSessionFactoryProvider provider;
        private const int TenantId = 10155;
        private const int UserId = 28000;
        private const int RoleId = 10115;
        private const int PartyId = 3500000;
        private const int TaskTypeId = 57881;
        private readonly Mock<IServiceHttpClientFactory> clientFactory = new Mock<IServiceHttpClientFactory>();
        private readonly Mock<IServiceHttpClient> client = new Mock<IServiceHttpClient>();

        public SeedWithTestData(IReadWriteSessionFactoryProvider provider)
        {
            this.provider = provider;
        }

        public override object Execute(IDatabaseSettings settings)
        {
            var claimsIdentity = new IntelliFloClaimsIdentity("Test", "Bearer");
            claimsIdentity.AddClaim(new Claim(Constants.ApplicationClaimTypes.UserId, TenantId.ToString(CultureInfo.InvariantCulture)));
            claimsIdentity.AddClaim(new Claim(Constants.ApplicationClaimTypes.TenantId, TenantId.ToString(CultureInfo.InvariantCulture)));
            Thread.CurrentPrincipal = new IntelliFloClaimsPrincipal(claimsIdentity);

            using (var session = provider.SessionFactory.OpenSession())
            using (var tx = session.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                var templateCategory = session.Query<TemplateCategory>().SingleOrDefault(t => t.TenantId == TenantId && t.Name == "Test");
                if (templateCategory == null)
                {
                    templateCategory = new TemplateCategory("Test", TenantId);
                    session.Save(templateCategory);
                }

                if (!session.Query<Template>().Any(b => b.TenantId == TenantId))
                {
                    session.Save(new Template("Test", TenantId, templateCategory, WorkflowRelatedTo.Client, UserId));
                    var draftTemplate = new Template("Draft", TenantId, templateCategory, WorkflowRelatedTo.Client, UserId);
                    session.Save(draftTemplate);

                    var activeTemplate = new Template("Active", TenantId, templateCategory, WorkflowRelatedTo.Client, UserId);
                    activeTemplate.SetRoles(new[] { RoleId });
                    activeTemplate.AddStep(new CreateTaskStep(Guid.NewGuid(), TaskTransition.OnCompletion, TaskTypeId, TaskAssignee.User, assignedToPartyId: PartyId));
                    activeTemplate.SetStatus(WorkflowStatus.Active);
                    session.Save(activeTemplate);

                    clientFactory.Setup(f => f.Create(It.IsAny<string>())).Returns(client.Object);
                    client.Setup(c => c.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoByUserId, UserId), null)).Returns(() => Task.FromResult(new HttpResponse<Dictionary<string, object>>()
                    {
                        Raw = new HttpResponseMessage(HttpStatusCode.OK),
                        Resource = new Dictionary<string, object>()
                        {
                            { Constants.ApplicationClaimTypes.UserId, UserId },
                            { Constants.ApplicationClaimTypes.TenantId, TenantId },
                            { Constants.ApplicationClaimTypes.PartyId, PartyId }
                        }
                    }));

                    var serviceFactory = new WorkflowServiceFactory(new DayDelayPeriod(), clientFactory.Object);
                    var xaml = serviceFactory.Build(activeTemplate);

                    session.Save(new TemplateDefinition() { Name = activeTemplate.Name, Id = activeTemplate.Guid, DateUtc = DateTime.UtcNow, TenantId = TenantId, Version = TemplateDefinition.DefaultVersion, Definition = xaml });
                }

                tx.Commit();
                return true;
            }
        }

        public override void Dispose() { }
    }
}