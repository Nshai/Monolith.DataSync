using System;
using System.Linq;
using Amazon.SimpleNotificationService.Model;
using IntelliFlo.Platform.Bus;
using IntelliFlo.Platform.NHibernate.Repositories;
using Microservice.DataSync.Domain;
using Microservice.DataSync.Messaging.Messages;
using Microservice.DataSync.v2.Contracts;


namespace Microservice.DataSync.v2.Resources.Impl
{
    
    public class DataSynceResource : IDataSyncResource
    {
        private readonly IRepository<DataSyncRequest> dataSyncRequestRepository;
        private readonly IBusPublisher busPublisher;

        public DataSynceResource(IRepository<DataSyncRequest> dataSyncRequestRepository, IBusPublisher busPublisher)
        {
            this.dataSyncRequestRepository = dataSyncRequestRepository;
            this.busPublisher = busPublisher;
        }


        public DataSyncRequestDocument Get(Guid requestId)
        {
            var request = dataSyncRequestRepository
                .Query()
                .FirstOrDefault(x => x.Id == requestId);

            if(request == null) throw new NotFoundException($"Sync request not found for {requestId}");

            return new DataSyncRequestDocument
            {
                Id = request.Id,
                PlanId = request.PlanId,
                TenantId = request.TenantId,
                PlanValue = request.PlanValue,
                ValuationDate = request.ValuationDate
            };
        }

        public DataSyncRequestDocument[] Get(int planId)
        {
            return dataSyncRequestRepository
                .Query()
                .Where(x => x.PlanId == planId)
                .Select(request => new DataSyncRequestDocument
                {
                    Id = request.Id,
                    PlanId = request.PlanId,
                    TenantId = request.TenantId,
                    PlanValue = request.PlanValue,
                    ValuationDate = request.ValuationDate
                })
                .ToArray();

        }

        public void SyncRequest(DataSyncRequestDocument request)
        {
            var syncRequest = new DataSyncRequest
            {
                Id = request.Id,
                PlanId = request.PlanId,
                TenantId = request.TenantId,
                PlanValue = request.PlanValue,
                ValuationDate = request.ValuationDate
            };

            // save the sync request to db.
            dataSyncRequestRepository.Save(syncRequest);

            // publish command to process the sync request.
            busPublisher.Publish(new SyncDataCommand
            {
                Id = request.Id,
                PlanId = request.PlanId,
                TenantId = request.TenantId,
                PlanValue = request.PlanValue,
                ValuationDate = request.ValuationDate
            });
        }

        public void ProcessSync(SyncDataCommand request)
        {
        }
    }
}
