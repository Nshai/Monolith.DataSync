using System;
using System.Activities.Tracking;
using IntelliFlo.Platform.NHibernate.Repositories;
using log4net;
using Microservice.Workflow.Domain;
using NHibernate;

namespace Microservice.Workflow.Engine
{
    public class DatabaseTrackingParticipant : TrackingParticipant
    {
        private readonly ISessionFactory sessionFactory;
        private readonly ILog logger = LogManager.GetLogger(typeof(DatabaseTrackingParticipant));

        /// <summary>
        /// Updates instance history
        /// </summary>
        /// <param name="sessionFactory"></param>
        public DatabaseTrackingParticipant(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            using (var session = sessionFactory.OpenSession())
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
                                    break;
                                case "Aborted":
                                case "Terminated":
                                    if (SetInstanceStatus(instanceRepository, record.InstanceId, InstanceStatus.Aborted))
                                    {
                                        instanceHistory = InstanceHistory.Aborted(record.InstanceId, record.EventTime);
                                    }
                                    break;
                                case "UnhandledException":
                                    if (SetInstanceStatus(instanceRepository, record.InstanceId, InstanceStatus.Errored))
                                    {
                                        var unhandledException = record as WorkflowInstanceUnhandledExceptionRecord;
                                        if (unhandledException != null)
                                        {
                                            var errorId = ExceptionLogger.Log(unhandledException.UnhandledException);

                                            instanceHistory = InstanceHistory.Errored(record.InstanceId, record.EventTime);
                                            instanceHistory.Data = new LogData
                                            {
                                                Detail = new ExceptionLog
                                                {
                                                    ErrorId = errorId
                                                }
                                            };
                                        }
                                    }
                                    break;
                                case "Unsuspended":
                                    SetInstanceStatus(instanceRepository, record.InstanceId, InstanceStatus.Processing);
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

                            if (activityRecord.Data.ContainsKey("IsComplete"))
                                instanceHistory.IsComplete = (bool) activityRecord.Data["IsComplete"];
                            else
                                instanceHistory.IsComplete = true;


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

        private bool SetInstanceStatus(IRepository<Instance> instanceRepository, Guid instanceId, InstanceStatus status)
        {
            var instance = instanceRepository.Get(instanceId);

            if (instance == null) return false;
            instance.Status = status.ToString();
            instance.UniqueId = Guid.NewGuid();
            instanceRepository.Update(instance);

            return true;
        }
    }
}