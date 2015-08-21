using System;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class InstanceHistoryDocument : Representation
    {
        public int InstanceHistoryId { get; set; }
        public Guid InstanceId { get; set; }
        public Guid StepId { get; set; }
        public string Step { get; set; }
        public LogData Data { get; set; }
        public bool IsComplete { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string Href
        {
            get
            {
                return LinkTemplates.InstanceHistory.Self.CreateLink(new { version = LocalConstants.ServiceVersion1, instanceId = InstanceId.ToString("N") }).Href;
            }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.InstanceHistory.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia(){}
    }
}
