using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Workflow.Migrator.Collaborators
{
    public class InstanceStepDocument : IRepresentation
    {
        public Guid InstanceStepId { get; set; }
        public Guid InstanceId { get; set; }
        public int StepIndex { get; set; }
        public string Step { get; set; }
        public LogData[] Data { get; set; }
        public bool IsComplete { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
