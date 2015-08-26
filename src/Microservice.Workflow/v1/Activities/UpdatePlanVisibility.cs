using System.Activities;
using System.Linq;
using IntelliFlo.Platform.Http.Client;
using Microservice.Workflow.Collaborators.v1;
using Constants = Microservice.Workflow.Engine.Constants;

namespace Microservice.Workflow.v1.Activities
{
    public sealed class UpdatePlanVisibility : NativeActivity
    {
        private static class PlanTypes
        {
            public static readonly int[] NotVisibleToClient = { Cpb, Introducer, Renewal, Undetermined, ConveyancingServicingPlan, Will, TaxPlanning, PowerOfAttorney, Trust };

            private const int Cpb = 61;
            private const int Introducer = 69;
            private const int Renewal = 70;
            private const int Undetermined = 81;
            private const int ConveyancingServicingPlan = 88;
            private const int Will = 138;
            private const int TaxPlanning = 1078;
            private const int PowerOfAttorney = 1102;
            private const int Trust = 1103;
        }

        private static class PlanStatus
        {
            public static readonly string[] VisibleStatus = new[] { "In force", "Paid Up" };
            public const string Draft = "Draft";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var workflowContext = (WorkflowContext) context.Properties.Find(WorkflowConstants.WorkflowContextKey);

            using (UserContextBuilder.FromBearerToken(workflowContext.BearerToken))
            {
                var clientFactory = IoC.Resolve<IServiceHttpClientFactory>(Constants.ContainerId);

                using (var portfolioClient = clientFactory.Create("portfolio"))
                {
                    HttpResponse<PlanDocument> planResponse = null;
                    var planTask = portfolioClient.Get<PlanDocument>(string.Format(Uris.Portfolio.GetPlan, workflowContext.ClientId, workflowContext.EntityId))
                        .ContinueWith(t =>
                        {
                            t.OnException(s => { throw new HttpClientException(s); });
                            planResponse = t.Result;
                        });

                    planTask.Wait();
                    var plan = planResponse.Resource;
                    var isExcludedType = PlanTypes.NotVisibleToClient.Contains(plan.PlanTypeId);

                    // Switch off flags for new plans (when excluded type)
                    if (plan.CurrentStatus == PlanStatus.Draft && isExcludedType)
                    {
                        this.LogMessage(context, LogLevel.Info, "Clear plan visibility for plan (id: {0}) as type is excluded", workflowContext.EntityId);
                        PatchPlan(portfolioClient, workflowContext.ClientId, workflowContext.EntityId, new PlanPatchRequest()
                        {
                            IsVisibilityUpdatedByStatusChange = false,
                            IsVisibleToClient = false
                        });
                        return;
                    }

                    // Set plan visibility
                    var setVisible = PlanStatus.VisibleStatus.Contains(plan.CurrentStatus) && !isExcludedType;
                    if (plan.IsVisibilityUpdatedByStatusChange)
                    {
                        this.LogMessage(context, LogLevel.Info, "Set plan visibility {0} for plan (id: {1})", setVisible ? "on" : "off", workflowContext.EntityId);
                        PatchPlan(portfolioClient, workflowContext.ClientId, workflowContext.EntityId, new PlanPatchRequest()
                        {
                            IsVisibleToClient = setVisible,
                            IsVisibilityUpdatedByStatusChange = !isExcludedType
                        });
                    }
                }
            }
        }

        private static void PatchPlan(IServiceHttpClient httpClient, int clientId, int planId, PlanPatchRequest request)
        {
            var planTask = httpClient.Patch<PlanDocument, PlanPatchRequest>(string.Format(Uris.Portfolio.PatchPlan, clientId, planId), request)
                .ContinueWith(t =>
                {
                    t.OnException(s => { throw new HttpClientException(s); });
                });

            planTask.Wait();
        }
    }
}
