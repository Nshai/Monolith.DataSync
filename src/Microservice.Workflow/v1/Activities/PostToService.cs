using System.Activities;
using System.Net.Http;
using System.Text;
using IntelliFlo.Platform.Http.Client;
using Constants = Microservice.Workflow.Engine.Constants;

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
            body = body.Replace("{EntityId}", workflowContext.EntityId.ToString());

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            
            using (UserContextBuilder.FromBearerToken(workflowContext.BearerToken))
            {
                var clientFactory = IoC.Resolve<IServiceHttpClientFactory>(Constants.ContainerId);
                using (var client = clientFactory.Create(serviceName))
                {
                    var task = client.Post(uri, content).ContinueWith(t =>
                    {
                        t.OnException(status => { throw new HttpClientException(status); });
                    });

                    task.Wait();
                }
            }
        }
    }
}