﻿using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.v1.Contracts;

namespace IntelliFlo.Platform.Services.Workflow.v1
{
    public interface ITemplateCategoryResource : IResource
    {
        PagedResult<TemplateCategoryDocument> Query(string query, IDictionary<string, object> routeValues);

        TemplateCategoryDocument Get(int templateCategoryId);
        TemplateCategoryDocument Post(CreateTemplateCategoryRequest request);
        TemplateCategoryDocument Patch(int templateCategoryId, TemplateCategoryPatchRequest request);
        void Delete(int templateCategoryId);
    }
}