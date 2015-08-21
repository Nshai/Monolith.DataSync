using System;
using System.Activities;
using System.Data.SqlClient;
using System.ServiceModel;
using IntelliFlo.Platform.NHibernate.Repositories;
using IntelliFlo.Platform.Services.Workflow.Domain;
using log4net;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Exceptions;
using Constants = IntelliFlo.Platform.Services.Workflow.Engine.Constants;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public sealed class RegisterInstance : NativeActivity, ILogActivity
    {
        public InArgument<WorkflowContext> WorkflowContext { get; set; }
        public InArgument<string> EntityType { get; set; }
        public InArgument<Guid> TemplateId { get; set; }

        private readonly ILog logger = LogManager.GetLogger(typeof(RegisterInstance));
        private const int SqlDuplicateExceptionNumber = 2601;

        protected override void Execute(NativeActivityContext context)
        {
            var ctx = WorkflowContext.Get(context);
            var templateId = TemplateId.Get(context);
            var entityType = EntityType.Get(context);
            var instanceId = context.WorkflowInstanceId;

            using (var userContext = UserContextBuilder.FromBearerToken(ctx.BearerToken))
            {
                logger.InfoFormat("Registering instance {0}", instanceId);

                var checkForDuplicates = ctx.PreventDuplicates;

                try
                {
                    var sessionFactory = IoC.Resolve<ISessionFactory>(Constants.ContainerId);
                    using (var session = sessionFactory.OpenSession())
                    {
                        var templateDefinitionRepository = new NHibernateRepository<TemplateDefinition>(session);
                        var instanceRepository = new NHibernateRepository<Instance>(session);

                        using (var tx = session.BeginTransaction())
                        {
                            try
                            {
                                var instance = new Instance
                                {
                                    Id = instanceId,
                                    Template = templateDefinitionRepository.Load(templateId),
                                    CorrelationId = ctx.CorrelationId,
                                    EntityType = entityType,
                                    EntityId = ctx.EntityId,
                                    RelatedEntityId = ctx.RelatedEntityId,
                                    Status = InstanceStatus.Processing.ToString(),
                                    UserId = userContext.Value.UserId,
                                    TenantId = userContext.Value.TenantId,
                                    CreateDate = DateTime.UtcNow,
                                    UniqueId = checkForDuplicates ? Guid.Empty : Guid.NewGuid(),
                                    Version = TemplateDefinition.DefaultVersion
                                };

                                if (ctx.ClientId > 0)
                                {
                                    instance.ParentEntityType = "Client";
                                    instance.ParentEntityId = ctx.ClientId;
                                }

                                instanceRepository.Save(instance);
                            }
                            catch (Exception)
                            {
                                tx.Rollback();
                                throw;
                            }

                            tx.Commit();
                        }
                    }
                }
                catch (GenericADOException ex)
                {
                    if (ex.InnerException != null)
                    {
                        var sqlException = ex.InnerException as SqlException;
                        if (sqlException != null && sqlException.Number == SqlDuplicateExceptionNumber)
                            throw new FaultException(string.Format("Cannot create duplicate instance of workflow"), new FaultCode(FaultCodes.DuplicateInstance));
                    }

                    throw;
                }
            }

            // Don't log the creation if we are running to a specific step
            if (!string.IsNullOrEmpty(ctx.AdditionalContext))
            {
                var additionalContext = JsonConvert.DeserializeObject<AdditionalContext>(ctx.AdditionalContext);
                var runToContext = additionalContext.RunTo;
                if (runToContext != null)
                    return;
            }

            this.LogMessage(context, StepName.Created.ToString());
        }
    }
}