using System;
using System.Activities;
using System.Activities.Tracking;
using System.Threading;
using IntelliFlo.Platform.Principal;
using log4net;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Activities
{
    public static class LogExtensions
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(LogExtensions).Name);

        public static void LogMessage(this Activity activity, NativeActivityContext context, bool complete = true, LogData data = null)
        {
            var step = context.Properties.Find(WorkflowConstants.WorkflowStepNameKey).ToString();
            LogMessage(activity, context, step, complete, data);
        }

        public static void LogMessage(this Activity activity, NativeActivityContext context, string stepName, bool complete = true, LogData data = null)
        {
            var stepId = context.Properties.Find(WorkflowConstants.WorkflowStepIdKey);
            var stepIndex = context.Properties.Find(WorkflowConstants.WorkflowStepIndexKey);

            var record = new CustomTrackingRecord(stepName);
            record.Data.Add("StepId", stepId == null ? Guid.NewGuid() : (Guid)stepId);
            
            if(stepIndex != null)
                record.Data.Add("StepIndex", (int)stepIndex);

            record.Data.Add("IsComplete", complete);
            if(data != null)
                record.Data.Add("Data", data);
            context.Track(record);

            LogMessage(activity, context, "{0}{1}{2}", stepIndex != null ? string.Format("{0}. ", stepIndex) : "", stepName, complete ? " completed" : "");
        }

        public static void LogMessage(this Activity activity, NativeActivityContext context, string message, params object[] args)
        {
            AddDefaultLogInfo(context);
            logger.InfoFormat(message, args);
        }

        private static void AddDefaultLogInfo(NativeActivityContext context)
        {
            LogicalThreadContext.Properties["correlationId"] = context.WorkflowInstanceId;

            if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.AsIFloPrincipal() != null)
            {
                var principal = Thread.CurrentPrincipal.AsIFloPrincipal();
                LogicalThreadContext.Properties["userId"] = principal.UserId;
                LogicalThreadContext.Properties["tenantId"] = principal.TenantId;
                LogicalThreadContext.Properties["subject"] = principal.Subject;
            }
        }
    }
}