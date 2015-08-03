namespace IntelliFlo.Platform.Services.Workflow.Collaborators.v1
{
    public static class Uris
    {
        public static class Self
        {
            public const string TriggerInstance = "v1/templates/{0}/createinstance/ontrigger";
            public const string AbortInstance = "v1/instances/{0}/abort";
            public const string ResumeInstance = "v1/instances/{0}/resume?bookmarkName={1}";
        }

        public static class Crm
        {
            public const string GetAdviser = "v1/advisers/{0}";
            public const string GetClient = "v1/clients/{0}";
            public const string GetServiceCase = "v1/clients/{0}/servicecases/{1}";
            public const string CreateTask = "v1/tasks";
            public const string GetLead = "v1/leads/{0}";
            public const string GetUserInfoBySubject = "v1/claims/subject/{0}";
            public const string GetUserInfoByUserId = "v1/claims/user/{0}";
        }

        public static class Portfolio
        {
            public const string GetPlan = "v1/clients/{0}/plans/{1}";
            public const string PatchPlan = "v1/clients/{0}/plans/{1}";
        }

        public static class EventManagement
        {
            public const string Post = "v1/subscriptions";
            public const string Delete = "v1/subscriptions/{0}";
            public const string DeleteForMigration = "v1/admin/subscriptions/{0}";
            public const string CreateSubscription = "v1/admin/subscriptions";
        }

        public static class Holidays
        {
            public const string Get = "v1/holidays?from={0}&to={1}";
        }
    }
}