using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.v1
{
    public interface ITemplateResource : IResource
    {
        TemplateDocument Get(int templateId);
        TemplateDocument Post(CreateTemplateRequest request);
        TemplateDocument Patch(int templateId, TemplatePatchRequest request);
        void Delete(int templateId);
        void CreateInstance(int templateId, CreateInstanceRequest request, bool triggeredInstance = false);
        void CreateInstance(string templateIdentifier, CreateInstanceRequest request);
        Template GetTemplate(int templateId);
        PagedResult<TemplateDocument> Query(string query, IDictionary<string, object> routeValues);
        TemplateDocument Clone(int templateId, CloneTemplateRequest request);
        PagedResult<TemplateExtDocument> QueryExt(string query, IDictionary<string, object> routeValues);
        TemplateExtDocument GetExt(int templateId);
        void Initialise(int templateId);
        TemplateRegistrationDocument Register(string identifier, RegisterTemplateRequest request);
        void Unregister(string identifier);
    }
}