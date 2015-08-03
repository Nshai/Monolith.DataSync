using System;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class TemplateCategoryInUseException : Exception
    {
        public string CategoryName { get; set; }

        public TemplateCategoryInUseException(string categoryName)
        {
            CategoryName = categoryName;
        }
    }
}