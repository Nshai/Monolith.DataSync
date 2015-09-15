﻿using IntelliFlo.Platform;
using IntelliFlo.Platform.Database;
using IntelliFlo.Platform.Database.Impl;
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
                DbOptions options;
                host.ConfigureDb(out options)
                    .Service<DefaultMicroService>(service =>
                    {
                        service.ConstructUsing(() => new DefaultMicroService(Settings.Default)
                            .WithContainer(s => new ContainerStartup(s))
                            .WithApi(s => new ApiStartup())
                            .WithDb(s => new DefaultDbStartup(options, s))
                            .With("workflow", s => new WorkflowStartup(s)));
                        service.WhenStarted(a => a.Start());
                        service.WhenStopped(a => a.Stop());
                    });

                host.SetDescription("Hosts user-defined and system workflows");
                host.SetDisplayName("Microservice.Workflow");
                host.SetServiceName("Microservice.Workflow");

                host.RunAsNetworkService();
            });
        }
    }
}