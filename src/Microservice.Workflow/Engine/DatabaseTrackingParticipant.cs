using System;
using System.Activities.Tracking;
using System.Net;
using System.Threading;
using IntelliFlo.Platform;
using IntelliFlo.Platform.NHibernate;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Principal;
using log4net;
using Microservice.Workflow.Domain;
using NHibernate;

namespace Microservice.Workflow.Engine
{
    public class DatabaseTrackingParticipant : TrackingParticipant
    {
        private readonly IReadWriteSessionFactoryProvider sessionFactory;
        private readonly ILog logger = LogManager.GetLogger(typeof(DatabaseTrackingParticipant));

        /// <summary>
        /// Updates instance history
        /// </summary>
        /// <param name="sessionFactory"></param>
        public DatabaseTrackingParticipant(IReadWriteSessionFactoryProvider sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            using (var session = sessionFactory.SessionFactory.OpenSession())
            {
                var instanceRepository = new NHibernateRepository<Instance>(session);
                var instanceHistoryRepository = new NHibernateRepository<InstanceHistory>(session);

                using (var tx = session.BeginTransaction())
                {
                    try
                    {
                        InstanceHistory instanceHistory = null;

                        var completedRecord = record as WorkflowInstanceRecord;
                        if (completedRecord != null)
                        {
                            switch (completedRecord.State)
                            {
                                case "Completed":
                                    if (SetInstanceStatus(instanceRepository, record.InstanceId, InstanceStatus.Completed))
                                    {
                                        instanceHistory = InstanceHistory.Completed(record.InstanceId, record.EventTime);
                                    }
                                    LogMessage(record, LogLevel.Info, "Instance completed");
                                    break;
                                case "Aborted":
                                case "Terminated":
                                    if (SetInstanceStatus(instanceRepository, record.InstanceId, InstanceStatus.Aborted))
                                    {
                                        instanceHistory = InstanceHistory.Aborted(record.InstanceId, record.EventTime);

                                        var abortedRecord = record as WorkflowInstanceAbortedRecord;
                                        if (abortedRecord != null && abortedRecord.Reason.Contains(HttpStatusCode.Forbidden.ToString()))
                                        {
                                            instanceHistory.Data = new LogData
                                            {
                                                Detail = new AbortRequestLog
                                                {
                                                    UserRequested = 0, 
                                                    Reason = "Assigned User no longer has Access"
                                                }
                                            };

                                        }
                                    }
                                    
                                    LogMessage(record, LogLevel.Warning, "Instance aborted");
                                    break;
                                case "UnhandledException":
                                    var unhandledException = record as WorkflowInstanceUnhandledExceptionRecord;
                                    if (unhandledException != null)
                                    {
                                        var errorId = ExceptionLogger.Log(unhandledException.UnhandledException);
                                        if (SetInstanceStatus(instanceRepository, record.InstanceId, InstanceStatus.Errored))
                                        {
                                            instanceHistory = InstanceHistory.Errored(record.InstanceId, record.EventTime);
                                            instanceHistory.Data = new LogData
                                            {
                                                Detail = new ExceptionLog
                                                {
                                                    ErrorId = errorId
                                                }
                                            };

                                        }
                                        LogMessage(record, LogLevel.Error, "Instance errored (error_code={0})", errorId);
                                    }
                                    break;
                                case "Unsuspended":
                                    SetInstanceStatus(instanceRepository, record.InstanceId, InstanceStatus.InProgress);
                                    LogMessage(record, LogLevel.Warning, "Instance suspended");
                                    break;
                            }
                        }

                        var activityRecord = record as CustomTrackingRecord;
                        if (activityRecord != null)
                        {
                            var activityTypeName = activityRecord.Activity.TypeName;
                            var activityAssembly = typeof (ILogActivity).Assembly;
                            var activityType = activityAssembly.GetType(activityTypeName);
                            if (!typeof (ILogActivity).IsAssignableFrom(activityType)) return;

                            LogData data = null;
                            if (activityRecord.Data.ContainsKey("Data"))
                                data = (LogData) activityRecord.Data["Data"];

                            Guid stepId;
                            if (activityRecord.Data.ContainsKey("StepId"))
                                stepId = (Guid) activityRecord.Data["StepId"];
                            else
                                stepId = Guid.NewGuid();

                            instanceHistory = new InstanceHistory(record.InstanceId, stepId, activityRecord.Name, record.EventTime);

                            var isComplete = true;
                            if (activityRecord.Data.ContainsKey("IsComplete"))
                                isComplete = (bool)activityRecord.Data["IsComplete"];
                            
                            instanceHistory.IsComplete = isComplete;

                            if (data != null)
                                instanceHistory.Data = data;
                        }

                        if (instanceHistory != null)
                            instanceHistoryRepository.Save(instanceHistory);


                        tx.Commit();
                    }
                    catch (Exception)
                    {
                        tx.Rollback();
                    }
                }
            }
        }

        public void LogMessage(TrackingRecord record, LogLevel level, string message, params object[] args)
        {
            using (WithDefaultLogInfo(record))
            {
                switch (level)
                {
                    case LogLevel.Error:
                        logger.ErrorFormat(message, args);
                        break;
                    case LogLevel.Warning:
                        logger.WarnFormat(message, args);
                        break;
                    case LogLevel.Info:
                        logger.InfoFormat(message, args);
                        break;
                }
            }
        }

        private static DisposableAction WithDefaultLogInfo(TrackingRecord record)
        {
            LogicalThreadContext.Properties["correlationId"] = record.InstanceId;

            if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.AsIFloPrincipal() != null)
            {
                var principal = Thread.CurrentPrincipal.AsIFloPrincipal();
                LogicalThreadContext.Properties["userId"] = principal.UserId;
                LogicalThreadContext.Properties["tenantId"] = principal.TenantId;
                LogicalThreadContext.Properties["subject"] = principal.Subject;
            }

            return new DisposableAction(() => LogicalThreadContext.Properties.Clear());
        }

        private bool SetInstanceStatus(IRepository<Instance> instanceRepository, Guid instanceId, InstanceStatus status)
        {
            var instance = instanceRepository.Get(instanceId);

            if (instance == null) return false;
            instance.Status = status.ToPrettyString();
            instance.UniqueId = Guid.NewGuid();
            instanceRepository.Update(instance);

            return true;
        }
    }
}