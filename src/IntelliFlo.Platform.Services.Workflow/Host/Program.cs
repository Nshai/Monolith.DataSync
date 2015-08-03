using IntelliFlo.Platform.Services.Workflow.Properties;
using Topshelf;

namespace IntelliFlo.Platform.Services.Workflow.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostFactory.Run(host =>
            {
                host.Service<DefaultMicroService>(service =>
                {
                    service.ConstructUsing(() =>
                        new DefaultMicroService(Settings.Default)
                            .WithContainer(s => new ContainerStartup(s))
                            .WithApi(s => new ApiStartup())
                            .With("workflow", s => new WorkflowStartup(s)));
                    service.WhenStarted(a => a.Start());
                    service.WhenStopped(a => a.Stop());
                });

                host.SetDescription("Hosts user-defined and system workflows");
                host.SetDisplayName("MicroService.Workflow");
                host.SetServiceName("MicroService.Workflow");

                host.RunAsNetworkService();
            });
        }
    }
}
