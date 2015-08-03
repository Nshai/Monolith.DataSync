using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Exceptions;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;

namespace IntelliFlo.Platform.Services.Workflow.v1.Controllers
{
    [Authorize]
    [BadRequestOnException(typeof(AssertionFailedException), typeof(BusinessException), typeof(ValidationException))]
    [NotFoundOnException(typeof(EntityNotFoundException))]
    [ForbiddenOnException(typeof(ForbiddenException), typeof(TemplatePermissionsException))]
    [RoutePrefix("v1/templates/{templateId}/roles")]
    public class TemplateRoleController : ApiController
    {
        private readonly ITemplateRoleResource templateRoleResource;

        public TemplateRoleController(ITemplateRoleResource templateRoleResource)
        {
            this.templateRoleResource = templateRoleResource;
        }

        /// <summary>
        /// List template roles
        /// </summary>
        [Route]
        public IHttpActionResult<TemplateRoleCollection> GetAll(int templateId)
        {
            try
            {
                var roles = templateRoleResource.ListRoles(templateId);
                return Request.CreateTypedResult(HttpStatusCode.OK, roles);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateRoleCollection>(HttpStatusCode.NotFound, "Template not found");
            }
        }

        [Route]
        public IHttpActionResult<TemplateRoleCollection> Put(int templateId, IEnumerable<int> roleIds)
        {
            try
            {
                var roles = templateRoleResource.PutRoles(templateId, roleIds);
                return Request.CreateTypedResult(HttpStatusCode.OK, roles);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateRoleCollection>(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateNotUpdatableException)
            {
                return Request.CreateTypedResult<TemplateRoleCollection>(HttpStatusCode.Gone, "Template cannot be modified with existing instances");
            }
        }
    }
}
