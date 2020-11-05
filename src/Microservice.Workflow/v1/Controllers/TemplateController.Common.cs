using System.Net;
using System.Web.Http;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Exceptions;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1.Controllers
{
    public partial class TemplateController
    {
        /// <summary>
        /// Create instance for specific template
        /// </summary>
        /// <param name="templateIdentifier"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("common/{templateIdentifier}/createinstance")]
        [ValidateModel]
        public NoContentActionResult CreateInstance(string templateIdentifier, CreateInstanceRequest request)
        {
            try
            {
                templateResource.CreateInstance(templateIdentifier, request);
                return Request.CreateNoContentResult(HttpStatusCode.NoContent);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template not found");
            }
        }

        /// <summary>
        /// Create instance for specific template
        /// </summary>
        /// <param name="templateIdentifier"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("common/{templateIdentifier}")]
        [ValidateModel]
        public IHttpActionResult<TemplateRegistrationDocument> Register(string templateIdentifier, RegisterTemplateRequest request)
        {
            try
            {
                var registration = templateResource.Register(templateIdentifier, request);
                return Request.CreateTypedResult(HttpStatusCode.OK, registration);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateTypedResult<TemplateRegistrationDocument>(HttpStatusCode.NotFound, "Template not found");
            }
        }

        /// <summary>
        /// Create instance for specific template
        /// </summary>
        /// <param name="templateIdentifier"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("common/{templateIdentifier}")]
        [ValidateModel]
        public NoContentActionResult Unregister(string templateIdentifier)
        {
            try
            {
                templateResource.Unregister(templateIdentifier);
                return Request.CreateNoContentResult(HttpStatusCode.NoContent);
            }
            catch (TemplateNotFoundException)
            {
                return Request.CreateNoContentResult(HttpStatusCode.NotFound, "Template not found");
            }
        }

    }
}
