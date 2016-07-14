using System.Activities;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Http.Client.Policy;
using Microservice.Workflow.Host;

namespace Microservice.Workflow.v1.Activities
{
    public class PostToService : NativeActivity
    {
        public InArgument<string> ServiceName { get; set; }
        public InArgument<string> Uri { get; set; }
        public InArgument<string> Body { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var serviceName = ServiceName.Get(context);
            var uri = Uri.Get(context);
            var body = Body.Get(context);

            var workflowContext = (WorkflowContext)context.Properties.Find(WorkflowConstants.WorkflowContextKey);

            StringContent bodyContent = null;
            if (!string.IsNullOrEmpty(body))
            {
                body = body.Replace("{EntityId}", workflowContext.EntityId.ToString());
                bodyContent = new StringContent(body, Encoding.UTF8, "application/json");
            }

            using (var lifetimeScope = IoC.Container.BeginLifetimeScope(WorkflowScopes.Scope))
            using (UserContextBuilder.FromBearerToken(workflowContext.BearerToken, lifetimeScope))
            {
                PostInternal(serviceName, bodyContent, uri, lifetimeScope);
            }
        }

        private static void PostInternal(string serviceName, StringContent bodyContent, string uri, ILifetimeScope lifetimeScope)
        {
            var clientFactory = lifetimeScope.Resolve<IHttpClientFactory>();
            using (var client = clientFactory.Create(serviceName))
            {
                Task task;
                if (bodyContent != null)
                {
                    task =
                        client.UsingPolicy(HttpClientPolicy.Retry)
                            .SendAsync(c => c.Post(uri, bodyContent))
                            .ContinueWith(t => { t.OnException(status => { throw new HttpClientException(status); }); });
                }
                else
                {
                    task =
                        client.UsingPolicy(HttpClientPolicy.Retry)
                            .SendAsync(c => c.Post(uri))
                            .ContinueWith(t => { t.OnException(status => { throw new HttpClientException(status); }); });
                }
                task.Wait();
            }
        }
    }
}