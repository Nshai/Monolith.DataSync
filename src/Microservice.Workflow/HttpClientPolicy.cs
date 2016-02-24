using System;
using System.Net;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using Polly;

namespace Microservice.Workflow
{
    public static class HttpClientPolicy
    {
        public static Policy Retry
        {
            get
            {
                return Policy
                    .Handle<TaskCanceledException>()
                    .Or<HttpStatusException>(e => e.StatusCode == HttpStatusCode.ServiceUnavailable)
                    .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(10 * i));
            }
        }
    }
}
