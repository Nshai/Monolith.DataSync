using System.Net;
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
    [BadRequestOnException(typeof(AssertionFailedException), typeof(BusinessException), typeof(ValidationException))]
    [RoutePrefix("v1/templatecategories")]
    public class TemplateCategoryController : ApiController
    {
        private readonly ITemplateCategoryResource templateCategoryResource;

        public TemplateCategoryController(ITemplateCategoryResource templateCategoryResource)
        {
            this.templateCategoryResource = templateCategoryResource;
        }

        /// <summary>
        /// Get all campaign templates for the given tenant
        /// </summary>
        /// <returns>Paged collection of template documents</returns>
        [Route]
        [ODataTop, ODataSkip]
        [ODataFilter("Name", "IsArchived", Operators = AllowedLogicalOperators.All)]
        [ODataOrderBy]
        public IHttpActionResult<PagedRepresentationCollection<TemplateCategoryDocument>> GetAll()
        {
            return Request.CreatePagedTypedResultWithFilter<TemplateCategory, TemplateCategoryDocument>(HttpStatusCode.OK,
                (query, routeValues) => templateCategoryResource.Query(query, routeValues),
                ODataDescriptor.GetODataDescriptor<TemplateCategoryController>(a => a.GetAll()));
        }

        [Route("{templateCategoryId}")]
        public IHttpActionResult<TemplateCategoryDocument> Get(int templateCategoryId)
        {
            try
            {
                var category = templateCategoryResource.Get(templateCategoryId);
                return Request.CreateTypedResult(HttpStatusCode.OK, category);
            }
            catch (TemplateCategoryNotFoundException)
            {
                return Request.CreateTypedResult<TemplateCategoryDocument>(HttpStatusCode.NotFound, "Template category not found");
            }
        }

        [Route]
        [ValidateModel]
        public IHttpActionResult<TemplateCategoryDocument> Post(CreateTemplateCategoryRequest request)
        {
            try 
            {
                var category = templateCategoryResource.Post(request);
                return Request.CreateTypedResultForCreated(HttpStatusCode.Created, category, t => t.TemplateCategoryId);    
            }
            catch (TemplateCategoryNotUniqueException)
            {
                return Request.CreateTypedResult<TemplateCategoryDocument>(HttpStatusCode.BadRequest, "Category name must be unique");
            }
        }

        [Route("{templateCategoryId}")]
        public IHttpActionResult Delete(int templateCategoryId)
        {
            try
            {
                templateCategoryResource.Delete(templateCategoryId);
                return Request.CreateNoContentResult(HttpStatusCode.NoContent);
            }
            catch (TemplateCategoryNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template category not found");
            }
            catch (TemplateCategoryInUseException ex)
            {
                return Request.CreateNoContentResult(HttpStatusCode.BadRequest, string.Format("Category '{0}' is in use and cannot be deleted. Please archive.", ex.CategoryName));
            }
        }

        [Route("{templateCategoryId}")]
        [ValidateModel]
        public IHttpActionResult<TemplateCategoryDocument> Patch(int templateCategoryId, TemplateCategoryPatchRequest request)
        {
            try
            {
                var category = templateCategoryResource.Patch(templateCategoryId, request);
                return Request.CreateTypedResult(HttpStatusCode.OK, category);
            }
            catch (TemplateCategoryNotFoundException)
            {
                return Request.CreateTypedResult<TemplateCategoryDocument>(HttpStatusCode.NotFound, "Template category not found");
            }
            catch (TemplateCategoryPermissionsException)
            {
                return Request.CreateTypedResult<TemplateCategoryDocument>(HttpStatusCode.Forbidden, "Not permitted to update this category");
            }
            catch (TemplateCategoryNotUniqueException)
            {
                return Request.CreateTypedResult<TemplateCategoryDocument>(HttpStatusCode.BadRequest, "Category name must be unique");
            }
        }
    }
}
