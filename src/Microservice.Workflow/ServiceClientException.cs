using System;

namespace Microservice.Workflow
{
    public class ServiceClientException : Exception
    {
        public ServiceClientException(string message) : base(message){}
    }
}
