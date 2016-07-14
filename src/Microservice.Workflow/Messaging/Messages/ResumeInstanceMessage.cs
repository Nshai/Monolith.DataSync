using System;
using IntelliFlo.Platform.Bus;

namespace Microservice.Workflow.Messaging.Messages
{
    public class ResumeInstance : BusMessage
    {
        public Guid InstanceId { get; set; }
        public string Bookmark { get; set; }
    }
}
