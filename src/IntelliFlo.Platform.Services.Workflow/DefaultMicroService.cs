using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Common.Logging;
using IntelliFlo.Platform.Logging;

namespace IntelliFlo.Platform.Services.Workflow
{
    /// <summary>
    /// Overridden DefaultMicroService to allow easier addition of custom services
    /// </summary>
    public class DefaultMicroService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IStartup containerStartup;
        private readonly IMicroServiceSettings microServiceSettings;

        private Func<IMicroServiceSettings, IStartup> containerStartupBuilder;
        private readonly IDictionary<string, Func<IMicroServiceSettings, IStartup>> serviceBuilders;
        private readonly IDictionary<string, IStartup> services;


        public DefaultMicroService(ApplicationSettingsBase settingsBase)
        {
            GlobalLoggingContext.Initialise();
            microServiceSettings = GetMicroserviceSettings(settingsBase);
            serviceBuilders = new Dictionary<string, Func<IMicroServiceSettings, IStartup>>()
            {
                { "healthchecks", s => new DefaultHealthChecks(s)}
            };
            services = new Dictionary<string, IStartup>();
        }

        public DefaultMicroService WithContainer(Func<IMicroServiceSettings, IStartup> containerStartupBuilder)
        {
            this.containerStartupBuilder = containerStartupBuilder;
            return this;
        }

        public DefaultMicroService WithBus(Func<IMicroServiceSettings, IStartup> func)
        {
            return With("bus", func);
        }

        public DefaultMicroService WithApi(Func<IMicroServiceSettings, IStartup> func)
        {
            return With("api", func);
        }

        public DefaultMicroService WithScheduler(Func<IMicroServiceSettings, IStartup> func)
        {
            return With("scheduler", func);
        }

        public DefaultMicroService With(string service, Func<IMicroServiceSettings, IStartup> func)
        {
            if(serviceBuilders.ContainsKey(service))
                serviceBuilders.Remove(service);

            serviceBuilders.Add(service, func);
            return this;
        }

        /// <summary>
        /// Register custom healtchecks. If not supplied, default <see cref="DefaultHealthChecks"/> is used
        /// </summary>
        public DefaultMicroService WithHealthChecks(Func<IMicroServiceSettings, IStartup> func)
        {
            return With("healthchecks", func);
        }

        private IMicroServiceSettings GetMicroserviceSettings(ApplicationSettingsBase settingsBase)
        {
            Func<string, string> getKey = prop => ((string)(settingsBase[prop]));

            return new MicroServiceSettings(
                getKey("BaseAddress"),
                getKey("Service"),
                getKey("Instance"),
                getKey("Environment"),
                getKey("AltBaseAddress")
                );
        }

        public void Start()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("------------- IntelliFlo Platform Service -------------");
                Console.WriteLine("-------------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine(" SERVICE: " + microServiceSettings.Service);
                Console.WriteLine();

                InitialiseContainer();

                HibernateProfiler.StartNhibernateProfiler();

                serviceBuilders.ForEach(k => StartService(k.Key, k.Value));
                
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------------------");
            }
            catch (Exception e)
            {
                log.FatalFormat("Unhandled exception starting up microservice {0}", microServiceSettings.Service);
                log.Fatal(e);
                throw;
            }
        }

        private void StartService(string serviceName, Func<IMicroServiceSettings, IStartup> func)
        {
            if (func == null)
                return;
            log.InfoFormat("Initialising {0}...", serviceName);

            var service = func(microServiceSettings);
            services.Add(serviceName, service);
            service.Start();
        }

        private void StopService(string serviceName, IStartup service)
        {
            if (service == null)
                return;

            log.InfoFormat("Shutting down {0}...", serviceName);
            service.Dispose();
        }

        private void InitialiseContainer()
        {
            if (containerStartupBuilder == null)
            {
                log.InfoFormat("Custom container not supplied, using built in '{0}'", typeof(DefaultContainerStartup).FullName);
                containerStartupBuilder = (s) => new DefaultContainerStartup(microServiceSettings);
            }
            log.InfoFormat("Initialising container...");
            containerStartup = containerStartupBuilder(microServiceSettings);
            containerStartup.Start();
        }

        public void Stop()
        {
            services.Reverse().ForEach(k => StopService(k.Key, k.Value));

            StopContainer();

            HibernateProfiler.StopNhibernateProfiler();
        }

        private void StopContainer()
        {
            if (containerStartup == null)
            {
                log.InfoFormat("Container was not initialised...");
                return;
            }

            log.InfoFormat("Shutting down container...");
            containerStartup.Dispose();
        }
    }
}