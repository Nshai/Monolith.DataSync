﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
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
            var signManager = lifetimeScope.Resolve<ISignManager>();
            var signAuthenticationMessageBuilder = lifetimeScope.Resolve<ISignAuthenticationMessageBuilder>();
            var message = signManager.ExtractFromToken(token);
            var claimsDictionary = signAuthenticationMessageBuilder.ExtractOriginalMessage(message);
            return claimsDictionary.Select(c => new Claim(c.Key, c.Value));
        }
    }
}
