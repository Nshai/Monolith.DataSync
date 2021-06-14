using System;
using IntelliFlo.Platform.Bus;

namespace Microservice.DataSync.Messaging.Messages
{
    public class SyncDataCommand : BusMessage
    {
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public int PlanId { get; set; }
        public decimal PlanValue { get; set; }
        public DateTime ValuationDate { get; set; }
    }
}
