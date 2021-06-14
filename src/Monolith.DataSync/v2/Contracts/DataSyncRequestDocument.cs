using System;
using IntelliFlo.Platform.Http;

namespace Microservice.DataSync.v2.Contracts
{
    public class DataSyncRequestDocument : Representation
    {
        public Guid Id { get; set; }
       
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public int PlanId { get; set; }
        public decimal PlanValue { get; set; }
        public DateTime ValuationDate { get; set; }

       
        public override string Href
        {
            get
            {
                return LinkTemplates.Instance.Self
                    .CreateLink(new {version = LocalConstants.ServiceVersion2, instanceId = Id.ToString("N")}).Href;
            }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.Instance.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.Instance.History.CreateLink(new
            {
                version = LocalConstants.ServiceVersion2, instanceId = Id.ToString("N")
            }));
        }
    }

    public class LinkTemplates
    {
        public static class Instance
        {
            public static Link Self
            {
                get { return new Link("self", "~/v{version}/datasync/request/{planId}"); }
            }
            public static Link History
            {
                get { return new Link("history", "~/v{version}/datasync/request/{planId}/history"); }
            }
        }
    }
    public static class LocalConstants
    {
        public const string ServiceVersion2 = "v2";
    }
}
