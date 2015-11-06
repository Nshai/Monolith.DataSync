using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
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
    [BadRequestOnException(typeof(AssertionFailedException), typeof(BusinessException), typeof(ValidationException))]
    [RoutePrefix("v1/templates")]
    public partial class TemplateController : ApiController
    {
        private readonly ITemplateResource templateResource;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateResource"></param>
        public TemplateController(ITemplateResource templateResource)
        {
            this.templateResource = templateResource;
        }


        /// <summary>
        /// Initialise workflow service for specified template
        /// </summary>
        /// <remarks>Can be used to confirm template is working correctly</remarks>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{templateId}/initialise")]
        public NoContentActionResult Initialise(int templateId)
        {
            try
            {
                templateResource.Initialise(templateId);
                return Request.CreateNoContentResult(HttpStatusCode.NoContent);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateNotActiveException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.BadRequest, "Template not active");
            }
            catch (TemplatePermissionsException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.Forbidden, "Not permitted to create this instance");
            }
        }

        /// <summary>
        /// Create instance for specific template
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{templateId}/createinstance/ondemand")]
        [ValidateModel]
        public NoContentActionResult CreateInstance(int templateId, CreateInstanceRequest request)
        {
            try
            {
                templateResource.CreateInstance(templateId, request);
                return Request.CreateNoContentResult(HttpStatusCode.NoContent);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateNotActiveException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.BadRequest, "Template not active");
            }
            catch (TemplatePermissionsException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.Forbidden, "Not permitted to create this instance");
            }
            catch (DuplicateInstanceException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.BadRequest, "Not permitted to create a duplicate instance");
            }
        }
        
        /// <summary>
        /// Create an instance as a result of event trigger firing
        /// </summary>
        /// <remarks>Internal use only</remarks>
        /// <param name="templateId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{templateId}/createinstance/ontrigger")]
        [ValidateModel]
        [ApiExplorerSettings(IgnoreApi = true)]
        public NoContentActionResult CreateTriggeredInstance(int templateId, CreateInstanceRequest request)
        {
            try
            {
                templateResource.CreateInstance(templateId, request, true);
                return Request.CreateNoContentResult(HttpStatusCode.NoContent);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateNotActiveException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.BadRequest, "Template not active");
            }
            catch (DuplicateInstanceException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.BadRequest, "Not permitted to create a duplicate instance");
            }
        }

        /// <summary>
        /// Get template
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [Route("{templateId}")]
        public IHttpActionResult<TemplateDocument> Get(int templateId)
        {
            try
            {
                var template = templateResource.Get(templateId);
                return Request.CreateTypedResult(HttpStatusCode.OK, template);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateDocument>(HttpStatusCode.NotFound, "Template not found");
            }
        }

        /// <summary>
        /// Get template with additional detail on steps - saving need for multiple calls
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [Route("{templateId}/detailed")]
        public IHttpActionResult<TemplateExtDocument> GetExt(int templateId)
        {
            try
            {
                var template = templateResource.GetExt(templateId);
                return Request.CreateTypedResult(HttpStatusCode.OK, template);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateExtDocument>(HttpStatusCode.NotFound, "Template not found");
            }
        }

        /// <summary>
        /// Get all campaign templates for the given tenant
        /// </summary>
        /// <returns>Paged collection of template documents</returns>
        [Route]
        [ODataTop, ODataSkip]
        [ODataFilter("Id", "Name", "Status", "RelatedTo", "Category/Name", Operators = AllowedLogicalOperators.All)]
        [ODataOrderBy]
        public IHttpActionResult<PagedRepresentationCollection<TemplateDocument>> GetAll()
        {
            return Request.CreatePagedTypedResultWithFilter<Template, TemplateDocument>(HttpStatusCode.OK,
                (query, routeValues) => templateResource.Query(query, routeValues),
                ODataDescriptor.GetODataDescriptor<TemplateController>(a => a.GetAll()));
        }

        /// <summary>
        /// Get all campaign templates for the given tenant with additional details of steps
        /// </summary>
        /// <returns>Paged collection of template documents</returns>
        [Route("detailed")]
        [ODataTop, ODataSkip]
        [ODataFilter("Id", "Name", "Status", "RelatedTo", "Category/Name", Operators = AllowedLogicalOperators.All)]
        [ODataOrderBy]
        public IHttpActionResult<PagedRepresentationCollection<TemplateExtDocument>> GetExt()
        {
            return Request.CreatePagedTypedResultWithFilter<Template, TemplateExtDocument>(HttpStatusCode.OK,
                (query, routeValues) => templateResource.QueryExt(query, routeValues),
                ODataDescriptor.GetODataDescriptor<TemplateController>(a => a.GetAll()));
        }

        /// <summary>
        /// Create new workflow template
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route]
        [ValidateModel]
        public IHttpActionResult<TemplateDocument> Post(CreateTemplateRequest request)
        {
            try
            {
                var template = templateResource.Post(request);
                return Request.CreateTypedResultForCreated(HttpStatusCode.Created, template, t => t.Id);
            }
            catch (TemplateNotUniqueException)
            {
                return Request.CreateTypedResult<TemplateDocument>(HttpStatusCode.BadRequest, "Template name must be unique");
            }
        }

        /// <summary>
        /// Clone an existing workflow template
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{templateId}/clone")]
        [ValidateModel]
        public IHttpActionResult<TemplateDocument> Clone(int templateId, CloneTemplateRequest request)
        {
            try
            {
                var template = templateResource.Clone(templateId, request);
                return Request.CreateTypedResultForCreated(HttpStatusCode.Created, template, t => t.Id);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateDocument>(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateNotUniqueException)
            {
                return Request.CreateTypedResult<TemplateDocument>(HttpStatusCode.BadRequest, "Template name must be unique");
            }
        }

        /// <summary>
        /// Delete an workflow template
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [Route("{templateId}")]
        public IHttpActionResult Delete(int templateId)
        {
            try
            {
                templateResource.Delete(templateId);
                return Request.CreateNoContentResult(HttpStatusCode.NoContent);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateNotUpdatableException ex)
            {
                var message = "Template is currently in use and cannot be deleted. Please archive.";
                if (ex.IsArchived)
                    message = "Template is currently in use and cannot be deleted.";

                return Request.CreateNoContentResult(HttpStatusCode.Gone, message);
            }
        }

        /// <summary>
        /// Perform partial update of workflow template
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("{templateId}")]
        [ValidateModel]
        public IHttpActionResult<TemplateDocument> Patch(int templateId, TemplatePatchRequest request)
        {
            try
            {
                var template = templateResource.Patch(templateId, request);
                return Request.CreateTypedResult(HttpStatusCode.OK, template);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateDocument>(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplatePermissionsException)
            {
                return Request.CreateTypedResult<TemplateDocument>(HttpStatusCode.Forbidden, "Not permitted to update this template");
            }
            catch (TemplateNotUniqueException)
            {
                return Request.CreateTypedResult<TemplateDocument>(HttpStatusCode.BadRequest, "Template name must be unique");
            }
            catch (TemplateNotUpdatableException ex)
            {
                return Request.CreateTypedResult<TemplateDocument>(HttpStatusCode.Gone, ex.IsArchived ? "Archived template cannot be modified" : "Template cannot be modified with existing instances");
            }
            catch (TemplateCompilationException)
            {
                return Request.CreateTypedResult<TemplateDocument>(HttpStatusCode.BadRequest, "Xaml is not correctly formatted");
            }
        }
    }
}