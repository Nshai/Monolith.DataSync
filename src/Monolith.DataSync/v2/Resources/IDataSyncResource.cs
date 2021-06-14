using System;
using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using Microservice.DataSync.Messaging.Messages;
using Microservice.DataSync.v2.Contracts;

namespace Microservice.DataSync.v2.Resources
{
    public interface IDataSyncResource : IResource
    {
        DataSyncRequestDocument Get(Guid instanceId);
        DataSyncRequestDocument[] Get(int planId);
        void SyncRequest(DataSyncRequestDocument request);
        void ProcessSync(SyncDataCommand request);

    }
}
