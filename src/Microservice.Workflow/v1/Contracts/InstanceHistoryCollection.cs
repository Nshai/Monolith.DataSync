using System;
using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class InstanceHistoryCollection : RepresentationCollection<InstanceHistoryDocument>
    {
        private readonly Guid instanceId;

        public InstanceHistoryCollection(){}
        public InstanceHistoryCollection(IList<InstanceHistoryDocument> res, Guid instanceId): base(res)
        {
            this.instanceId = instanceId;
        }

        public override string Href
        {
            get { return LinkTemplates.InstanceHistory.Collection.CreateLink(new { version = LocalConstants.ServiceVersion1, id = instanceId }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.InstanceHistory.Collection.Rel; }
            set { }
        }

        protected override void CreateHypermedia(){}
    }
}