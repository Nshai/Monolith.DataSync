using System;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.DataSync.Domain
{
    public class PlanValuation : EqualityAndHashCodeProvider<PlanValuation, int>
    {
        private decimal planValue;
        private readonly PlanValueType valueType;

        protected PlanValuation()
        {
        }

        public PlanValuation(decimal planValue, PlanValueType valueType)
        {

            this.planValue = planValue;
            this.valueType = valueType;
        }

        public virtual DateTime ValueDate { get; set; }
        public virtual DateTime LastValuationDate { get; set; }



        public virtual decimal PlanValue
        {
            get { return planValue; }
            set { planValue = value; }
        }

        public virtual PlanValueType ValueType
        {
            get { return valueType; }
        }

        public virtual int PlanId { get; set; }

        public virtual long ClientUserId { get; set; }

        public virtual decimal SurrenderTransferValue
        {
            get { return surrenderTransferValue; }
            set { surrenderTransferValue = value; }
        }

        private decimal surrenderTransferValue;
    }
    public enum PlanValueType
    {
        Manual = 1,
        ElectronicBulkValuation = 2,
        ElectronicLiveRequest = 3,
        UnderlyingFundValues = 4,
        ElectronicScheduledRequest = 5,
        ManualBulkValuation = 6
    }
}
