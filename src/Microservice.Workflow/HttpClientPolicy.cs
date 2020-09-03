using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using Polly;

namespace Microservice.Workflow
{
    public static class HttpClientPolicy
    {
        private static List<HttpStatusCode> retryable = new List<HttpStatusCode>
        {
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.InternalServerError
        };

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

        public static Policy RetryOn500Error
        {
            get
            {
                return Policy
                    .Handle<TaskCanceledException>()
                    .Or<HttpStatusException>(e => retryable.Contains(e.StatusCode))
                    .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(10 * i));
            }
        }
    }
}
