using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliFlo.Platform.Bus;
using Microservice.Workflow.Messaging.Messages;

namespace Microservice.Workflow.Messaging.MessageHandlers
{
    public class CheckForNewCallsHandler : IMessageHandler<CheckForNewCalls>
    {
        public bool Handle(IntelliFlo.Platform.Bus.IBusContext context, CheckForNewCalls message)
        {
            Console.WriteLine($"{message.PublishedOn} : {message.Message}");
            return true;
        }
    }
}
