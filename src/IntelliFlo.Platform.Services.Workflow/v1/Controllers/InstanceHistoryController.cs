using System;
using System.Net;
using System.Threading;
using System.Web.Http;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Exceptions;
using IntelliFlo.Platform.Http.OData;
using IntelliFlo.Platform.Principal;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;

namespace IntelliFlo.Platform.Services.Workflow.v1.Controllers
{
    [Authorize]
    [RoutePrefix("v1/instances/{instanceId}/history")]
    [NotFoundOnException(typeof(EntityNotFoundException))]
    [UnAuthorizedOnException(typeof(UnAuthorizedException))]
    public class InstanceHistoryController : ApiController
    {
        private readonly IInstanceResource instanceResource;
        private readonly IInstanceHistoryResource instanceHistoryResource;

        public InstanceHistoryController(IInstanceResource instanceResource, IInstanceHistoryResource instanceHistoryResource)
        {
            this.instanceResource = instanceResource;
            this.instanceHistoryResource = instanceHistoryResource;
        }

        [Route]
        [ODataSkip, ODataTop]
        public IHttpActionResult<PagedRepresentationCollection<InstanceHistoryDocument>> Get(Guid instanceId)
        {
            var instance = instanceResource.Get(instanceId);

            if (instance == null)
                throw new EntityNotFoundException("Instance not found");
            if (instance.TenantId != Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
                throw new UnAuthorizedException("Not permitted to view this instance");

            return Request.CreatePagedTypedResultWithFilter<InstanceHistory, InstanceHistoryDocument>(HttpStatusCode.OK, (query, routeValues) => instanceHistoryResource.QueryHistory(query, routeValues), ODataDescriptor.GetODataDescriptor<InstanceHistoryController>(a => a.Get(instanceId)));
        }
    }
}