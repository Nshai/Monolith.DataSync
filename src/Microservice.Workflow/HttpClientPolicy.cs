using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;
using Polly;
using Polly.Retry;

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

        public static RetryPolicy<HttpResponse<T>> GetRetryOnUnavailableOnInternalErrorOnNotFound<T>()
        {

            return Policy
                .Handle<TaskCanceledException>()
                .OrResult<HttpResponse<T>>(e =>
                e.Raw.StatusCode == HttpStatusCode.ServiceUnavailable ||
                e.Raw.StatusCode == HttpStatusCode.InternalServerError ||
                e.Raw.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(10 * i));

        }
        public static RetryPolicy<HttpResponse<T>> GetRetryPolicy<T>()
        {
            return Policy
            .Handle<TaskCanceledException>()
            .OrResult<HttpResponse<T>>(e => retryable.Contains(e.Raw.StatusCode))
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(10 * i));
        }

    }
}
