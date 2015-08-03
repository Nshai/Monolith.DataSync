using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliFlo.Platform.Services.Workflow.Collaborators.v1
{
    public class AdviserDocument
    {
        public int AdviserId { get; set; }
        public int? ManagerId { get; set; }
        public int? TnCCoachId { get; set; }
        public int? TnCCoachPartyId { get; set; }
    }
}
