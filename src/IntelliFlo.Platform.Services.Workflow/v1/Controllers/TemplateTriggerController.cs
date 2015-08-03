using System.Net;
using System.Web.Http;
using AutoMapper;
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
    [RoutePrefix("v1/templates/{templateId}/triggers")]
    public class TemplateTriggerController : ApiController
    {
        private readonly ITemplateTriggerResource templateTriggerResource;

        public TemplateTriggerController(ITemplateTriggerResource templateTriggerResource)
        {
            this.templateTriggerResource = templateTriggerResource;
        }

        /// <summary>
        /// List template roles
        /// </summary>
        [Route]
        public IHttpActionResult<TemplateTriggerCollection> GetAll(int templateId)
        {
            try
            {
                var triggers = templateTriggerResource.Get(templateId);
                return Request.CreateTypedResult(HttpStatusCode.OK, triggers);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateTriggerCollection>(HttpStatusCode.NotFound, "Template not found");
            }
        }

        [Route]
        [ValidateModel]
        public IHttpActionResult<TemplateTriggerCollection> Post(int templateId, CreateTemplateTriggerRequest request)
        {
            try
            {
                var triggers = templateTriggerResource.Post(templateId, Mapper.Map<CreateTemplateTrigger>(request));
                return Request.CreateTypedResult(HttpStatusCode.OK, triggers);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateTriggerCollection>(HttpStatusCode.NotFound, "Template not found");
            }
            catch (TemplateNotUpdatableException)
            {
                return Request.CreateTypedResult<TemplateTriggerCollection>(HttpStatusCode.Gone, "Template cannot be modified with existing instances");
            }
        }
    }
}
