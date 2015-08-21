using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IntelliFlo.Platform.Http.Client;

namespace Microservice.Workflow
{
    public static class HttpClientErrorHandlingExtensions
    {
        public static Task<HttpResponseMessage> OnException(this Task<HttpResponseMessage> task, Action<HttpStatusCode> continuationAction)
        {
            var response = task.Result;
            if (!response.IsSuccessStatusCode)
                continuationAction.Invoke(response.StatusCode);
            return task;
        }

        public static HttpResponse<TResult> OnException<TResult>(this HttpResponse<TResult> response, Action<HttpStatusCode> continuationAction)
        {
            if (!response.Raw.IsSuccessStatusCode)
                continuationAction.Invoke(response.Raw.StatusCode);
            return response;
        }

        public static HttpResponse<TResult> OnNotFound<TResult>(this HttpResponse<TResult> response, Action continuationAction)
        {
            Invoke(response, HttpStatusCode.NotFound, continuationAction);
            return response;
        }

        private static void Invoke<TResult>(HttpResponse<TResult> response, HttpStatusCode statusCode, Action continuationAction)
        {
            var httpStatusCode = response.Raw.StatusCode;
            if (httpStatusCode == statusCode)
                continuationAction.Invoke();
        }
    }
}
