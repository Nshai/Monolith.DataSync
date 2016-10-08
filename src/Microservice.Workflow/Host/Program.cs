using System.Linq;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Database;
using Microservice.Workflow.Properties;
using Topshelf;

namespace Microservice.Workflow.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostFactory.Run(host =>
            {
                // Append the new custom workflow scope, so that all modules would get this
                LifeTimeScopes.All = LifeTimeScopes.All.Union(new[] {WorkflowScopes.Scope}).ToArray();

                StartupOptions options;
                host.Configure(out options)
                    .Service<DefaultMicroService>(service =>
                    {
                        service.ConstructUsing(() => new DefaultMicroService(Settings.Default, options)
                            .WithContainer(s => new ContainerStartup(s))
                            .WithApi(s => new ApiStartup())
                            .WithDb(s => new DefaultDbStartup(s, options))
                            .WithBus(s => new BusStartup(s)));
                        service.WhenStarted(a => a.Start());
                        service.WhenStopped(a => a.Stop());
                    });

                host.SetDescription("Hosts user-defined and system workflows");
                host.SetDisplayName("Microservice.Workflow");
                host.SetServiceName("Microservice.Workflow");

                host.ConfigureServiceRecovery();
                host.RunAsNetworkService();
            });
        }
    }
}