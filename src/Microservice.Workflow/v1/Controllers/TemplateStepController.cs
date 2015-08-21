using System;
using System.Net;
using System.Web.Http;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Exceptions;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1.Controllers
{
    [Authorize]
    [BadRequestOnException(typeof (AssertionFailedException), typeof (BusinessException), typeof (ValidationException))]
    [NotFoundOnException(typeof(EntityNotFoundException))]
    [ForbiddenOnException(typeof(ForbiddenException), typeof(TemplatePermissionsException))]
    [RoutePrefix("v1/templates/{templateId}/steps")]
    public class TemplateStepController : ApiController
    {
        private readonly ITemplateStepResource templateStepResource;

        public TemplateStepController(ITemplateStepResource templateStepResource)
        {
            this.templateStepResource = templateStepResource;
        }

        /// <summary>
        /// List template steps
        /// </summary>
        [Route]
        public IHttpActionResult<TemplateStepCollection> GetAll(int templateId)
        {
            try
            {
                var steps = templateStepResource.List(templateId);
                return Request.CreateTypedResult(HttpStatusCode.OK, steps);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateStepCollection>(HttpStatusCode.NotFound, "Template not found");
            }
        }

        /// <summary>
        /// Create template step
        /// </summary>
        /// <param name="templateId">Identifies the owning template</param>
        /// <param name="request">Step details</param>
        /// <returns></returns>
        [Route]
        public IHttpActionResult<TemplateStepDocument> Post(int templateId, CreateTemplateStepRequest request)
        {
            try
            {
                var templateStep = templateStepResource.Post(templateId, request);
                return Request.CreateTypedResult(HttpStatusCode.Created, templateStep);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateStepDocument>(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateNotUpdatableException)
            {
                return Request.CreateTypedResult<TemplateStepDocument>(HttpStatusCode.Gone, "Template cannot be modified with existing instances");
            }
        }

        [HttpPost]
        [Route("{stepId}/moveUp")]
        public IHttpActionResult<TemplateStepDocument> MoveStepUp(int templateId, Guid stepId)
        {
            try
            {
                var step = templateStepResource.MoveStepUp(templateId, stepId);
                return Request.CreateTypedResult(HttpStatusCode.OK, step);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateStepDocument>(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateStepNotFoundException)
            {
                return Request.CreateTypedResult<TemplateStepDocument>(HttpStatusCode.NotFound, "Template step not found");
            }
            catch (TemplateNotUpdatableException)
            {
                return Request.CreateTypedResult<TemplateStepDocument>(HttpStatusCode.Gone, "Template cannot be modified with existing instances");
            }
        }

        [HttpPost]
        [Route("{stepId}/moveDown")]
        public IHttpActionResult<TemplateStepDocument> MoveStepDown(int templateId, Guid stepId)
        {
            try
            {
                var step = templateStepResource.MoveStepDown(templateId, stepId);
                return Request.CreateTypedResult(HttpStatusCode.NoContent, step);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateStepDocument>(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateStepNotFoundException)
            {
                return Request.CreateTypedResult<TemplateStepDocument>(HttpStatusCode.NotFound, "Template step not found");
            }
            catch (TemplateNotUpdatableException)
            {
                return Request.CreateTypedResult<TemplateStepDocument>(HttpStatusCode.Gone, "Template cannot be modified with existing instances");
            }
        }

        [Route("{stepId}")]
        public IHttpActionResult Patch(int templateId, Guid stepId, TemplateStepPatchRequest request)
        {
            try
            {
                var step = templateStepResource.Patch(templateId, stepId, request);
                return Request.CreateTypedResult(HttpStatusCode.OK, step);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateStepNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template step not found");
            }
            catch (TemplateNotUpdatableException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.Gone, "Template cannot be modified with existing instances");
            }
        }

        /// <summary>
        /// List template steps
        /// </summary>
        [Route("{stepId}")]
        public IHttpActionResult<TemplateStepDocument> Get(int templateId, Guid stepId)
        {
            var step = templateStepResource.Get(templateId, stepId);
            return Request.CreateTypedResult(HttpStatusCode.OK, step);
        }

        /// <summary>
        /// Delete a template step
        /// </summary>
        /// <param name="templateId">Identifies the owning template</param>
        /// <param name="stepId">Identifies the step to delete</param>
        /// <returns></returns>
        [Route("{stepId}")]
        public IHttpActionResult Delete(int templateId, Guid stepId)
        {
            try
            {
                templateStepResource.Delete(templateId, stepId);
                return Request.CreateNoContentResult(HttpStatusCode.NoContent, "Step was deleted");
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateNotUpdatableException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.Gone, "Template cannot be modified with existing instances");
            }
        }
    }
}