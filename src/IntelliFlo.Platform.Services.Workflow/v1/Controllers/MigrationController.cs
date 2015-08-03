using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData.Query;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Http.Exceptions;
using IntelliFlo.Platform.Http.OData;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;

namespace IntelliFlo.Platform.Services.Workflow.v1.Controllers
{
    [Authorize]
    [BadRequestOnException(typeof(AssertionFailedException), typeof(BusinessException), typeof(ValidationException))]
    [NotFoundOnException(typeof(EntityNotFoundException))]
    [RoutePrefix("v1/migrate")]
    public class MigrationController : ApiController
    {
        private readonly IMigrationResource migrationResource;

        public MigrationController(IMigrationResource migrationResource)
        {
            this.migrationResource = migrationResource;
        }

        [Route("templates")]
        [ODataTop, ODataSkip]
        [ODataFilter("Id", "Name", "Status", "RelatedTo", "Category/Name", Operators = AllowedLogicalOperators.All)]
        [ODataOrderBy]
        public IHttpActionResult<PagedRepresentationCollection<TemplateMigrationDocument>> GetTemplates()
        {
            return Request.CreatePagedTypedResultWithFilter<Template, TemplateMigrationDocument>(HttpStatusCode.OK,
                (query, routeValues) => migrationResource.GetTemplates(query, routeValues),
                ODataDescriptor.GetODataDescriptor<MigrationController>(a => a.GetTemplates()));
        }

        [Route("instances")]
        [ODataTop, ODataSkip]
        [ODataFilter("Id", "EntityType", "EntityId", "RelatedEntityId", "Status", "Template/Name", "CorrelationId", Operators = AllowedLogicalOperators.All)]
        [ODataOrderBy]
        public IHttpActionResult<PagedRepresentationCollection<InstanceDocument>> GetInstances()
        {
            return Request.CreatePagedTypedResultWithFilter<Instance, InstanceDocument>(HttpStatusCode.OK,
                (query, routeValues) => migrationResource.GetInstances(query, routeValues),
                ODataDescriptor.GetODataDescriptor<MigrationController>(a => a.GetInstances()));
        }
        
        [HttpPost]
        [Route("templates/{templateId}")]
        [ValidateModel]
        public async Task<IHttpActionResult<TemplateMigrationResponse>> MigrateTemplate(int templateId)
        {
            var response = await migrationResource.MigrateTemplate(templateId);
            return Request.CreateTypedResult(HttpStatusCode.OK, response);
        }

        [HttpPost]
        [Route("instances/{instanceId}")]
        [ValidateModel]
        public async Task<IHttpActionResult<InstanceMigrationResponse>> MigrateInstance(Guid instanceId)
        {
            var response = await migrationResource.MigrateInstance(instanceId);
            return Request.CreateTypedResult(HttpStatusCode.OK, response);
        }
    }
}
