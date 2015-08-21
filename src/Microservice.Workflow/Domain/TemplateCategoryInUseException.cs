using System;

namespace Microservice.Workflow.Domain
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