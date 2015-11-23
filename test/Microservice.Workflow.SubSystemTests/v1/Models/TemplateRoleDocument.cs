using IntelliFlo.Platform.NHibernate.Validation;

namespace Microservice.Workflow.SubSystemTests.v1.Models
{
    public class TemplateRoleDocument
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int TemplateVersionId { get; set; }
        public int RoleId { get; set; }
    }
}
