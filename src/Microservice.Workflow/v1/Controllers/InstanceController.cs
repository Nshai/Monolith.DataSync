using System;
using System.Net;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.OData.Query;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Exceptions;
using IntelliFlo.Platform.Http.OData;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1.Controllers
{
    [Authorize]
    [RoutePrefix("v1/instances")]
    [BadRequestOnException(typeof(AssertionFailedException), typeof(BusinessException), typeof(ValidationException))]
    [NotFoundOnException(typeof(EntityNotFoundException))]
    [ForbiddenOnException(typeof(ForbiddenException))]
    [GoneOnException(typeof(NoLongerAvailableException))]
    public class InstanceController : ApiController
    {
        private readonly IInstanceResource instanceResource;

        public InstanceController(IInstanceResource instanceResource)
        {
            this.instanceResource = instanceResource;
        }

        [HttpPost]
        [Route("{instanceId}/resume")]
        public IHttpActionResult<InstanceDocument> Resume(Guid instanceId, string bookmarkName)
        {
            try
            {
                var instance = instanceResource.Resume(instanceId, bookmarkName);
                return Request.CreateTypedResult(HttpStatusCode.Accepted, instance);
            }
            catch (ServerTooBusyException ex)
            {
                return Request.CreateTypedResult<InstanceDocument>(HttpStatusCode.ServiceUnavailable, ex.Message);
            }
            catch (InstanceNotFoundException)
            {
                throw new EntityNotFoundException("Instance not found");
            }
            catch (InstancePermissionsException)
            {
                throw new ForbiddenException("Not permitted to resume this instance");
            }
        }

        [HttpPost]
        [Route("{instanceId}/abort")]
        public NoContentActionResult Abort(Guid instanceId)
        {
            try
            {
                instanceResource.Abort(instanceId);
            }
            catch (ServerTooBusyException ex)
            {
                return Request.CreateNoContentResult(HttpStatusCode.ServiceUnavailable, ex.Message);
            }
            catch (InstanceNotFoundException)
            {
                throw new EntityNotFoundException("Instance not found");
            }
            catch (InstancePermissionsException)
            {
                throw new ForbiddenException("Not permitted to abort this instance");
            }
            catch (InstanceAbortException)
            {
                throw new NoLongerAvailableException("Instance cannot be aborted");
            }
            return Request.CreateNoContentResult(HttpStatusCode.Accepted);
        }

        [HttpPost]
        [Route("{instanceId}/restart")]
        public IHttpActionResult<InstanceDocument> Restart(Guid instanceId)
        {
            try
            {
                var newInstance = instanceResource.Restart(instanceId);
                return Request.CreateTypedResult(HttpStatusCode.Created, newInstance);
            }
            catch (ServerTooBusyException ex)
            {
                return Request.CreateTypedResult<InstanceDocument>(HttpStatusCode.ServiceUnavailable, ex.Message);
            }
            catch (InstanceNotFoundException)
            {
                throw new EntityNotFoundException("Instance not found");
            }
            catch (InstancePermissionsException)
            {
                throw new ForbiddenException("Not permitted to restart this instance");
            }
            catch (InstanceNotRestartableException)
            {
                return Request.CreateTypedResult<InstanceDocument>(HttpStatusCode.BadRequest, "Instance not restartable");
            }
        }

        [HttpPost]
        [Route("{instanceId}/unsuspend")]
        public NoContentActionResult Unsuspend(Guid instanceId)
        {
            try
            {
                instanceResource.Unsuspend(instanceId);
            }
            catch (ServerTooBusyException ex)
            {
                return Request.CreateNoContentResult(HttpStatusCode.ServiceUnavailable, ex.Message);
            }
            catch (InstanceNotFoundException)
            {
                throw new EntityNotFoundException("Instance not found");
            }
            catch (InstancePermissionsException)
            {
                throw new ForbiddenException("Not permitted to unsuspend this instance");
            }
            catch (InstanceAbortException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.Gone, "Instance cannot be unsuspend");
            }
            return Request.CreateNoContentResult(HttpStatusCode.Accepted);
        }

        [HttpGet]
        [Route("{instanceId}")]
        public IHttpActionResult<InstanceDocument> Get(Guid instanceId)
        {
            var instance = instanceResource.Get(instanceId);
            return Request.CreateTypedResult(HttpStatusCode.OK, instance);
        }

        [Route]
        [ODataTop, ODataSkip]
        [ODataFilter("EntityType", "EntityId", "RelatedEntityId", "Status", "Template/Name", "CorrelationId", Operators = AllowedLogicalOperators.All)]
        [ODataOrderBy]
        public IHttpActionResult<PagedRepresentationCollection<InstanceDocument>> Get()
        {
            return Request.CreatePagedTypedResultWithFilter<Instance, InstanceDocument>(HttpStatusCode.OK,
                (query, routeValues) => instanceResource.Query(query, routeValues),
                ODataDescriptor.GetODataDescriptor<InstanceController>(a => a.Get()));
        }
    }
}