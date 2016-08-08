using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using Autofac;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.Principal;

namespace Microservice.Workflow.v1.Activities
{
    public static class UserContextBuilder
    {
        public static DisposableAction<IIntelliFloClaimsPrincipal> FromBearerToken(string token, ILifetimeScope lifetimeScope)
        {
            var previous = Thread.CurrentPrincipal;

            var claims = ExtractClaims(token, lifetimeScope);
            var userIdClaim = claims.SingleOrDefault(c => c.Type == IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.UserId);
            if(userIdClaim == null)
                throw new ClaimNotFoundException(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.UserId);

            var identity = new IntelliFloClaimsIdentity(userIdClaim.Value, "Trusted");
            identity.AddClaims(claims);
            var principal = new IntelliFloClaimsPrincipal(identity);
            Thread.CurrentPrincipal = principal;

            return new DisposableAction<IIntelliFloClaimsPrincipal>(ctx =>
            {
                Thread.CurrentPrincipal = previous;
            }, principal);
        }

        private static IEnumerable<Claim> ExtractClaims(string token, ILifetimeScope lifetimeScope)
        {
            // We don't care whether the token has expired, we are just using it to extract the claims
            var message = ExtractFromToken(token);
            var signAuthenticationMessageBuilder = lifetimeScope.Resolve<ISignAuthenticationMessageBuilder>();
            
            var claimsDictionary = signAuthenticationMessageBuilder.ExtractOriginalMessage(message);
            return claimsDictionary.Select(c => new Claim(c.Key, c.Value));
        }

        private static string ExtractFromToken(string message)
        {
            Check.IsNotNullOrWhiteSpace(message, "message cannot be null or empty");

            var base64EncodedBytes = Convert.FromBase64String(message);
            var decodeMsg = Encoding.UTF8.GetString(base64EncodedBytes);

            return decodeMsg.Split('|')[0];
        }
    }
}
