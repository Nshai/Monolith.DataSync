using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;
using IntelliFlo.Platform.Http.Client;
using Microservice.Workflow.Collaborators.v1;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Activities;
using Check = IntelliFlo.Platform.Check;

namespace Microservice.Workflow.v1.Activities
{
    public class WorkflowServiceFactory : IWorkflowServiceFactory, IMapActivity
    {
        private readonly IDelayPeriod delayPeriod;
        private readonly IServiceHttpClientFactory clientFactory;
        private const string DynamicWorkflow = "DynamicWorkflow";
        private const string ActivityPresentationNs = "http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation";

        public WorkflowServiceFactory(IDelayPeriod delayPeriod, IServiceHttpClientFactory clientFactory)
        {
            this.delayPeriod = delayPeriod;
            this.clientFactory = clientFactory;
        }

        public string Build(Template template)
        {
            Check.IsNotNull(template, "Template must be supplied");

            var name = string.Format("{0}_{1}", DynamicWorkflow, template.Guid.ToString("N"));
            var workflowService = new System.ServiceModel.Activities.WorkflowService { ConfigurationName = DynamicWorkflow, Name = XName.Get(name) };

            var entityType = (EntityType)Enum.Parse(typeof(EntityType), template.RelatedTo.ToString());
            Check.IsTrue(entityType != EntityType.None, "Entity type could not be resolved");

            var rootSequence = new Sequence();

            var context = new Variable<WorkflowContext>("context");
            var instanceId = new Variable<Guid>("instanceId");

            var create = new Create
            {
                InstanceId = instanceId,
                Context = context,
                TemplateId = template.Guid,
                TemplateType = entityType.ToString()
            };

            rootSequence.Variables.Add(context);
            rootSequence.Variables.Add(instanceId);

            rootSequence.Activities.Add(create);

            var parentStep = new WorkflowStep()
            {
                EnableLogging = false,
                Context = context
            };

            rootSequence.Activities.Add(parentStep);

            var stepIndex = 0;
            foreach (var step in template.Steps)
            {
                Check.IsNotNull(step, "Step was empty");
                parentStep.Activities.Add(step.GetActivity(this, template, stepIndex));
                stepIndex++;
            }

            workflowService.Body = rootSequence;
            AddReferences(workflowService.Body);

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true }))
            using (var xamlWriter = ActivityXamlServices.CreateBuilderWriter(new XamlXmlWriter(writer, new XamlSchemaContext())))
            {
                XamlServices.Save(xamlWriter, workflowService);
            }

            var xaml = sb.ToString();
            var document = XDocument.Parse(xaml);
            RemoveIgnorableChildren(document, new[] { XNamespace.Get(ActivityPresentationNs) });
            return document.ToString();
        }

        public Activity Map(Domain.CreateTaskStep step, Template template, int stepIndex)
        {
            Check.IsTrue(step.TaskTypeId > 0, "Task type Id for Create Task is required");
            Check.IsTrue(template.OwnerUserId > 0, "Template must have owner user");

            var ownerPartyId = GetUserPartyId(template.OwnerUserId);

            var activity = new CreateTaskStep
            {
                StepId = step.Id,
                StepIndex = stepIndex,
                TaskTransition = step.Transition.ToString(),
                DueDelay = step.DueDelay,
                DueDelayBusinessDays = step.DueDelayBusinessDays,
                TaskTypeId = step.TaskTypeId,
                AssignedTo = step.AssignedTo.ToString(),
                TemplateOwnerPartyId = ownerPartyId,
                OwnerPartyId = step.AssignedToPartyId.HasValue ? step.AssignedToPartyId.Value : (InArgument<int>)null,
                OwnerRoleId = step.AssignedToRoleId.HasValue ? step.AssignedToRoleId.Value : (InArgument<int>)null,
                OwnerContextRole = step.AssignedToRoleContext.HasValue ? step.AssignedToRoleContext.ToString() : null
            };

            return activity;
        }

        public Activity Map(Domain.DelayStep step, Template template, int stepIndex)
        {
            var activity = new DelayStep
            {
                StepId = step.Id,
                StepIndex = stepIndex,
                Period = delayPeriod.GetPeriod(step.Days),
                BusinessDaysOnly = step.BusinessDays
            };

            return activity;
        }

        private int GetUserPartyId(int userId)
        {
            using (var crmClient = clientFactory.Create("crm"))
            {
                HttpResponse<Dictionary<string, object>> userInfoResponse = null;
                var userInfoTask = crmClient.Get<Dictionary<string, object>>(string.Format(Uris.Crm.GetUserInfoByUserId, userId))
                    .ContinueWith(t =>
                    {
                        t.OnException(status => { throw new HttpClientException(status); });
                        userInfoResponse = t.Result;
                    });

                userInfoTask.Wait();

                var claims = userInfoResponse.Resource;

                Check.IsTrue(claims.ContainsKey(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.PartyId), "Couldn't retrieve party id claim for user id {0}", userId);

                return int.Parse(claims[IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.PartyId].ToString());
            }
        }

        private static void RemoveIgnorableChildren(XDocument document, IEnumerable<XNamespace> namespaces)
        {
            Check.IsNotNull(document, "Document must be supplied");
            document.Descendants().Where(x => namespaces.Contains(x.Name.Namespace)).Remove();
            document.Descendants().SelectMany(x => x.Attributes()).Where(x => namespaces.Contains(x.Name.NamespaceName)).Remove();
        }

        private static void AddReferences(Activity body)
        {
            TextExpression.SetNamespaces(body,
                "System",
                "System.Collections.Generic",
                "System.Data",
                "System.Linq",
                "System.Text",
                "Microsoft.Activities"
                );

            TextExpression.SetReferences(body, new[]
            {
                new AssemblyReference {AssemblyName = new AssemblyName("Newtonsoft.Json")},
                new AssemblyReference {AssemblyName = new AssemblyName("PresentationCore")},
                new AssemblyReference {AssemblyName = new AssemblyName("PresentationFramework")},
                new AssemblyReference {AssemblyName = new AssemblyName("System")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Activities")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Activities.Presentation")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.ComponentModel.DataAnnotations")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Configuration")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Core")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Drawing")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.ServiceModel")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.ServiceModel.Activities")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Web")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Xaml")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Xml.Linq")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Data.DataSetExtensions")},
                new AssemblyReference {AssemblyName = new AssemblyName("Microsoft.CSharp")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Data")},
                new AssemblyReference {AssemblyName = new AssemblyName("System.Xml")},
                new AssemblyReference {AssemblyName = new AssemblyName("WindowsBase")},
                new AssemblyReference {AssemblyName = new AssemblyName("mscorlib")},
                new AssemblyReference {AssemblyName = new AssemblyName("MicroService.Workflow")}
            });
        }

        
    }
}