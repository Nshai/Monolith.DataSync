using System;
using System.Net;

namespace IntelliFlo.Platform.Services.Workflow
{
    [Serializable]
    public class HttpClientException : Exception
    {
        public HttpClientException(HttpStatusCode statusCode) : base(string.Format("Http request failed with {0}", statusCode)) { }
    }
}
