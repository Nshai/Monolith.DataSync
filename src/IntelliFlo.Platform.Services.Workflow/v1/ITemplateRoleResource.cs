using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;

namespace IntelliFlo.Platform.Services.Workflow.v1
{
    public interface ITemplateRoleResource : IResource
    {
        TemplateRoleCollection ListRoles(int templateId);
        TemplateRoleCollection PutRoles(int templateId, IEnumerable<int> roleIds);
    }
}