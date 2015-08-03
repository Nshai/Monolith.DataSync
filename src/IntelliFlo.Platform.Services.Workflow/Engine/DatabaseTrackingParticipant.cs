using System;
using System.Activities.Tracking;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Services.Workflow.Domain;
using NHibernate;

namespace IntelliFlo.Platform.Services.Workflow.Engine
{
    public class DatabaseTrackingParticipant : TrackingParticipant
    {
        private readonly IRepository<Instance> instanceRepository;
        private readonly IRepository<InstanceHistory> instanceHistoryRepository;
        private readonly ISession session;

        public DatabaseTrackingParticipant(IRepository<Instance> instanceRepository, IRepository<InstanceHistory> instanceHistoryRepository, ISession session)
        {
            this.instanceRepository = instanceRepository;
            this.instanceHistoryRepository = instanceHistoryRepository;
            this.session = session;
        }

        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
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
                                if (SetInstanceStatus(record.InstanceId, InstanceStatus.Completed))
                                {
                                    instanceHistory = InstanceHistory.Completed(record.InstanceId, record.EventTime);
                                }
                                break;
                            case "Aborted":
                            case "Terminated":
                                if (SetInstanceStatus(record.InstanceId, InstanceStatus.Aborted))
                                {
                                    instanceHistory = InstanceHistory.Aborted(record.InstanceId, record.EventTime);
                                }
                                break;
                            case "UnhandledException":
                                if (SetInstanceStatus(record.InstanceId, InstanceStatus.Errored))
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
                                SetInstanceStatus(record.InstanceId, InstanceStatus.Processing);
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

        private bool SetInstanceStatus(Guid instanceId, InstanceStatus status)
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