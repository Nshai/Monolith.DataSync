using System;
using System.ServiceModel.Configuration;

namespace IntelliFlo.Platform.Services.Workflow.Engine
{
    public class DatabaseTrackingExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(DatabaseTrackingBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new DatabaseTrackingBehavior();
        }
    }
}