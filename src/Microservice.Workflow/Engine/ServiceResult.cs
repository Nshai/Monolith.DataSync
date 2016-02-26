namespace Microservice.Workflow.Engine
{
    public class ServiceResult<T> : ServiceResult
    {
        public T Result { get; set; }
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
    }
}