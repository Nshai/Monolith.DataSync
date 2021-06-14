using System.Threading.Tasks;
using IntelliFlo.Platform.Bus;
using Microservice.DataSync.Messaging.Messages;
using Microservice.DataSync.v2.Resources;

namespace Microservice.DataSync.Messaging.MessageHandlers
{
    public class SyncDataCommandHandler : IMessageHandlerAsync<SyncDataCommand>
    {
        private readonly IDataSyncResource dataSyncResource;

        public SyncDataCommandHandler(IDataSyncResource dataSyncResource)
        {
            this.dataSyncResource = dataSyncResource;
        }


        public Task<bool> Handle(IBusContext context, SyncDataCommand message)
        {
            dataSyncResource.ProcessSync(message);
            return Task.FromResult(true);
        }
    }
}
