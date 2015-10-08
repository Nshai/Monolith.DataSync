using System;
using System.Linq;
using IdentityServer3.Core.Services.InMemory;

namespace Microservice.Workflow.SubSystemTests.Helpers
{
    public static class InMemoryUserExtensions
    {
        public static long? GetUserId(this InMemoryUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");


            return GetLongValue(user, "user_id");
        }

        public static string GetAccessToken(this InMemoryUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var oauthClient = Config.Server.UseResourceOwnerFlow(
                Config.Client.ClientId, "secret",
                user.Username, user.Password,
                new[] { "openid", "workflow" });

            return oauthClient.GetAccessToken();
        }

        public static long? GetPartyId(this InMemoryUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return GetLongValue(user, "party_id");
        }

        public static string GetFirstName(this InMemoryUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return GetStringValue(user, "given_name");
        }

        public static string GetLastName(this InMemoryUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return GetStringValue(user, "family_name");
        }

        private static string GetStringValue(InMemoryUser user, string key)
        {
            var claim = user.Claims.FirstOrDefault(x => x.Type == key);
            return claim == null ? null : claim.Value;
        }

        private static long? GetLongValue(InMemoryUser user, string key)
        {
            var claim = user.Claims.FirstOrDefault(x => x.Type == key);
            if (claim == null)
                return null;

            return long.Parse(claim.Value);
        }
    }
}
