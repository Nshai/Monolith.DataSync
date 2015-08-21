using System;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class InstanceStepDocument : Representation
    {
        public Guid InstanceStepId { get; set; }
        public Guid InstanceId { get; set; }
        public int StepIndex { get; set; }
        public string Step { get; set; }
        public LogData[] Data { get; set; }
        public bool IsComplete { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string Href
        {
            get
            {
                return LinkTemplates.InstanceStep.Self.CreateLink(new { version = LocalConstants.ServiceVersion1, instanceId = InstanceId.ToString("N") }).Href;
            }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.InstanceStep.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia() { }
    }
}
