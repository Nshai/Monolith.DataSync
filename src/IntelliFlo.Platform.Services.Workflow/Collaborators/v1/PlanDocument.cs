using System.Collections.Generic;

namespace IntelliFlo.Platform.Services.Workflow.Collaborators.v1
{
    public class PlanDocument
    {
        public int PlanId { get; set; }
        public int ClientId { get; set; }
        public int PlanTypeId { get; set; }
        public bool IsVisibilityUpdatedByStatusChange { get; set; }
        public bool IsVisibleToClient { get; set; }
        public IEnumerable<PlanOwner> Owners { get; set; }
        public int? SellingAdviserPartyId { get; set; }
        public int? ServicingAdministratorPartyId { get; set; }
        public int? TnCCoachPartyId { get; set; }
        public string CurrentStatus { get; set; }

        public class PlanOwner
        {
            public int ClientId { get; set; }
            public int PlanOwnerId { get; set; }
        }
    }
}
