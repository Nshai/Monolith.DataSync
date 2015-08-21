using System;
using System.Net;
using System.Threading;
using System.Web.Http;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Exceptions;
using IntelliFlo.Platform.Http.OData;
using IntelliFlo.Platform.Principal;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1.Controllers
{
    [Authorize]
    [RoutePrefix("v1/instances/{instanceId}/steps")]
    [NotFoundOnException(typeof(EntityNotFoundException))]
    [UnAuthorizedOnException(typeof(UnAuthorizedException))]
    public class InstanceStepController : ApiController
    {
        private readonly IInstanceResource instanceResource;
        private readonly IInstanceHistoryResource instanceHistoryResource;

        public InstanceStepController(IInstanceResource instanceResource, IInstanceHistoryResource instanceHistoryResource)
        {
            this.instanceResource = instanceResource;
            this.instanceHistoryResource = instanceHistoryResource;
        }

        [Route]
        [ODataSkip, ODataTop]
        [ODataOrderBy("StepIndex")]
        public IHttpActionResult<PagedRepresentationCollection<InstanceStepDocument>> Get(Guid instanceId)
        {
            var instance = instanceResource.Get(instanceId);

            if (instance == null)
                throw new EntityNotFoundException("Instance not found");
            if (instance.TenantId != Thread.CurrentPrincipal.AsIFloPrincipal().TenantId)
                throw new UnAuthorizedException("Not permitted to view this instance");

            return Request.CreatePagedTypedResultWithFilter<InstanceStep, InstanceStepDocument>(HttpStatusCode.OK, (query, routeValues) => instanceHistoryResource.QuerySteps(query, routeValues), ODataDescriptor.GetODataDescriptor<InstanceStepController>(a => a.Get(instanceId)));
        }
    }
}