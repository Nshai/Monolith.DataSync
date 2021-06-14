using System;
using System.Net;
using System.Web.Http;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Exceptions;
using Microservice.DataSync.v2.Contracts;
using Microservice.DataSync.v2.Resources;

namespace Microservice.DataSync.v2.Controllers
{
    [Authorize]
    [RoutePrefix("v2/datasync")]
    [BadRequestOnException(typeof(AssertionFailedException), typeof(BusinessException), typeof(ValidationException))]
    [NotFoundOnException(typeof(EntityNotFoundException))]
    [ForbiddenOnException(typeof(ForbiddenException))]
    [GoneOnException(typeof(NoLongerAvailableException))]
    public class DataSyncController : ApiController
    {
        private readonly IDataSyncResource dataSyncResource;

        public DataSyncController(IDataSyncResource dataSyncResource)
        {
            this.dataSyncResource = dataSyncResource;
        }

        [HttpGet]
        [Route("request/{requestId}")]
        public IHttpActionResult<DataSyncRequestDocument> Get(Guid requestId)
        {
            var requestDocument = dataSyncResource.Get(requestId);
            return Request.CreateTypedResult(HttpStatusCode.OK, requestDocument);
        }

        [HttpGet]
        [Route("request/{planId}/history")]
        public IHttpActionResult<DataSyncRequestDocument[]> Get(int planId)
        {
            var requestDocuments = dataSyncResource.Get(planId);
            return Request.CreateTypedResult(HttpStatusCode.OK, requestDocuments);
        }

        [HttpGet]
        [Route("request/{planId}/accept")]
        public IHttpActionResult SyncData(DataSyncRequestDocument request)
        {
            dataSyncResource.SyncRequest(request);
            return Request.CreateNoContentResult(HttpStatusCode.Accepted);
        }
    }
}
