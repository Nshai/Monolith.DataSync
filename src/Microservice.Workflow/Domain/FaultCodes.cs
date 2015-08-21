namespace Microservice.Workflow.Domain
{
    public static class FaultCodes
    {
        public const string InvalidContext = "InvalidContext";
        public const string InstanceNotFound = "InstanceNotFound";
        public const string CreateTaskFailed = "CreateTaskFailed";
        public const string DuplicateInstance = "DuplicateInstance";
        public const string SendEmailFailed = "SendEmailFailed";
    }
}