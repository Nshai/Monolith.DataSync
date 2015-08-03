using IntelliFlo.Platform.Http;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class LinkTemplates
    {
        public static class Instance
        {
            public static Link Self
            {
                get { return new Link("self", "~/workflow/v{version}/instances/{instanceId}"); }
            }

            public static Link History
            {
                get { return new Link("history", "~/workflow/v{version}/instances/{instanceId}/history"); }
            }
        }

        public static class InstanceHistory
        {
            public static Link Collection
            {
                get { return new Link("self", "~/workflow/v{version}/instances/{instanceId}/history"); }
            }

            public static Link Self
            {
                get { return new Link("self", "~/workflow/v{version}/instances/{instanceId}/history"); }
            }
        }

        public static class InstanceStep
        {
            public static Link Collection
            {
                get { return new Link("self", "~/workflow/v{version}/instances/{instanceId}/steps"); }
            }

            public static Link Self
            {
                get { return new Link("self", "~/workflow/v{version}/instances/{instanceId}/steps"); }
            }
        }

        public static class Template
        {
            public static Link Self
            {
                get { return new Link("self", "~/workflow/v{version}/templates/{templateId}"); }
            }
        }

        public static class TemplateCategory
        {
            public static Link Self
            {
                get { return new Link("self", "~/workflow/v{version}/templatecategories/{templateCategoryId}"); }
            }
        }

        public static class TemplateStep
        {
            public static Link Collection
            {
                get { return new Link("self", "~/workflow/v{version}/templates/{templateId}/steps"); }
            }
            public static Link Self
            {
                get { return new Link("self", "~/workflow/v{version}/templates/{templateId}/steps/{stepId}"); }
            }
            public static Link Template
            {
                get { return new Link("template", "~/workflow/v{version}/templates/{templateId}"); }
            }
        }

        public static class TemplateRole
        {
            public static Link Collection
            {
                get { return new Link("self", "~/workflow/v{version}/templates/{templateId}/roles"); }
            }
            public static Link Self
            {
                get { return new Link("self", "~/workflow/v{version}/templates/{templateId}/roles/{roleId}"); }
            }
            public static Link Template
            {
                get { return new Link("template", "~/workflow/v{version}/templates/{templateId}"); }
            }
        }

        public static class TemplateTrigger
        {
            public static Link Collection
            {
                get { return new Link("self", "~/workflow/v{version}/templates/{templateId}/triggers"); }
            }
            public static Link Self
            {
                get { return new Link("self", "~/workflow/v{version}/templates/{templateId}/triggers"); }
            }
            public static Link Template
            {
                get { return new Link("template", "~/workflow/v{version}/templates/{templateId}"); }
            }
        }

        public static class TemplateDefinition
        {
            public static Link Self
            {
                get { return new Link("self", "~/workflow/v{version}/templates/{templateId}/content"); }
            }
        }
    }
}
