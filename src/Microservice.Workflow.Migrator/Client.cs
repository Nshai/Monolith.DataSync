using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeContracts;
using Thinktecture.IdentityModel.Client;

namespace Microservice.Workflow.Migrator
{
    public class Client : IDisposable
    {
        private readonly OAuth2Client oauthClient;
        private readonly HttpClient client;
        private readonly List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>();
        private TokenResponse token;

        public Client()
        {
            var baseUri = new Uri(ConfigurationManager.AppSettings["api"]);

            client = new HttpClient() { BaseAddress = baseUri };
            formatters.Add(new JsonMediaTypeFormatter());

            var identityBaseUri = ConfigurationManager.AppSettings["identity"];
            Requires.True(Regex.IsMatch(identityBaseUri, "core/?$"), "Identity Server uri is incorrect");

            var tokenUri = new Uri(identityBaseUri + "/connect/token");
            oauthClient = new OAuth2Client(tokenUri, "wfmigrator", "RhwqGEGdv6P3VZFESMps");
        }

        public async Task InitialiseToken()
        {
            var response = await oauthClient.RequestClientCredentialsAsync("workflow");
            if(response.IsError)
                throw new Exception(string.Format("Failed to get access token: {0}", response.Error));

            token = response;
        }

        public async Task<TResource> Get<TResource>(string uri)
        {
            while (true)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                var accessToken = GetAccessToken();
                client.SetBearerToken(accessToken);

                var result = await client.SendAsync(request).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                    return await result.Content.ReadAsAsync<TResource>(formatters);
                if (result.StatusCode != HttpStatusCode.Unauthorized) 
                    throw new Exception(string.Format("GET {0} failed with HTTP {1} ", uri, (int) result.StatusCode));
                await InitialiseToken();
            }
        }
        
        public async Task<TResource> Post<TResource>(string uri)
        {
            while (true)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                var accessToken = GetAccessToken();
                client.SetBearerToken(accessToken);

                var result = await client.SendAsync(request).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                    return await result.Content.ReadAsAsync<TResource>(formatters);
                if (result.StatusCode != HttpStatusCode.Unauthorized)
                    throw new Exception(string.Format("POST {0} failed with HTTP {1} ", uri, (int)result.StatusCode));
                await InitialiseToken();
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }

        private string GetAccessToken()
        {
            return token != null ? token.AccessToken : "";
        }
    }
}
