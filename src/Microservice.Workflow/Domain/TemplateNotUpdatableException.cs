using System;

namespace Microservice.Workflow.Domain
{
    public class TemplateNotUpdatableException : Exception
    {
        public TemplateNotUpdatableException(bool isArchived = false)
        {
            IsArchived = isArchived;
        }

        public bool IsArchived { get; set; }
    }
}