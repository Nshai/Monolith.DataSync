using System;
using IntelliFlo.Platform.Bus;

namespace Microservice.Workflow.Messaging.Messages
{
    public class CheckForNewCalls : BusMessage
    {
        public DateTimeOffset PublishedOn { get; set; }
        public string Message { get; set; }
    }
}
