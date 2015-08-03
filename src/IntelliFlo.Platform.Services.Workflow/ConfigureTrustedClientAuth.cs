using System.Net.Http;
using System.Threading;
using IntelliFlo.Platform.Http.Client;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.Identity.Impl;
using IntelliFlo.Platform.Principal;

namespace IntelliFlo.Platform.Services.Workflow
{
    /// <summary>
    /// Need to override the Platform version which relies on having IIntelliFloPrincipal registered in IoC
    /// In workflow, we aren't running the Microservice pipeline, so this isn't registered instead we use the Thread.CurrentPrincipal
    /// </summary>
    public class ConfigureTrustedClientAuth : IHttpClientConfigurationMutator
    {
        private readonly ITrustedClientAuthenticationTokenBuilder trustedClientAuthenticationTokenBuilder;

        public ConfigureTrustedClientAuth(ITrustedClientAuthenticationTokenBuilder trustedClientAuthenticationTokenBuilder)
        {
            this.trustedClientAuthenticationTokenBuilder = trustedClientAuthenticationTokenBuilder;
        }

        public IHttpClientConfiguration Mutate(IHttpClientConfiguration config)
        {
            config.DelegatingHandlerFunc = () => new DelegatingHandler[]
            {
                new TrustedClientAuthenticationDelegatingHandler(trustedClientAuthenticationTokenBuilder, Thread.CurrentPrincipal.AsIFloPrincipal())
            };
            return config;
        }
    }
}