using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac;
using Common.Logging;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Bus;
using IntelliFlo.Platform.Bus.Scheduler;
using IntelliFlo.Platform.Bus.Serialization;
using JustSaying;
using JustSaying.Messaging.MessageSerialisation;
using Microservice.Workflow.Messaging.Messages;

namespace Microservice.Workflow.Host
{
    [ExcludeFromCodeCoverage]
    public class BusStartup : DefaultBusStartup<BusConfigurator>
    {
        public BusStartup(IMicroServiceSettings microServiceSettings)
            : base(microServiceSettings) {}

        public override void SetupContainer(ContainerBuilder builder)
        {
            base.SetupContainer(builder);
        }
    }

    [ExcludeFromCodeCoverage]
    public class BusConfigurator : DefaultBusConfigurator
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IBusNamingConvention busNamingConvention;
        private readonly IHandlerResolver handlerResolver;
        private const string SchedulerQueueName = "scheduler";
        private readonly int visibilityTimeoutSeconds = (int)TimeSpan.FromHours(12).TotalSeconds;

        public BusConfigurator(IBusNamingConvention busNamingConvention, IHandlerResolver handlerResolver)
        {
            this.busNamingConvention = busNamingConvention;
            this.handlerResolver = handlerResolver;
        }

        public override IMessageSerialisationFactory ConfigureSerialisationFactory()
        {
            return SerializationFactory
                .ForDefault(new NewtonsoftSerialiser())
                .Factory;
        }

        public override void RegisterHandlers(IMayWantOptionalSettings justSaying)
        {
            justSaying
                .WithSqsTopicSubscriber()
                .IntoQueue(busNamingConvention.QueueName())
                .WithMessageHandler<ResumeInstance>(handlerResolver)
                .WithSqsMessagePublisher<ScheduleTimeout>(c => c.QueueName = busNamingConvention.QueueName(SchedulerQueueName))
                .WithSqsMessagePublisher<UnscheduleTimeout>(c => c.QueueName = busNamingConvention.QueueName(SchedulerQueueName))
                .WithSqsMessagePublisher<ResumeInstance>(config => config.QueueName = busNamingConvention.QueueName());
        }
    }
}