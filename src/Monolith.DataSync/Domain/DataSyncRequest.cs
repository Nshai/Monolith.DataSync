using System;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.DataSync.Domain
{
    public class DataSyncRequest : EqualityAndHashCodeProvider<DataSyncRequest, Guid>
    {
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public int PlanId { get; set; }
        public decimal PlanValue { get; set; }
        public DateTime ValuationDate { get; set; }
    }
}
