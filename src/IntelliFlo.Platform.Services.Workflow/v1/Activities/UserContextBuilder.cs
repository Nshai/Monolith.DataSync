using System.Linq;
using System.Threading;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.Principal;
using IntelliFlo.Platform.Services.Workflow.Engine;
using IntelliFlo.Platform.Services.Workflow.Host;
using Constants = IntelliFlo.Platform.Services.Workflow.Engine.Constants;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public static class UserContextBuilder
    {
        public static DisposableAction<IIntelliFloClaimsPrincipal> FromBearerToken(string token)
        {
            var previous = Thread.CurrentPrincipal;

            var trustedClientAuthentication = IoC.Resolve<ITrustedClientAuthenticationScheme>(Constants.ContainerId);

            var claims = trustedClientAuthentication.Validate(token).Result;
            var userIdClaim = claims.SingleOrDefault(c => c.Type == Principal.Constants.ApplicationClaimTypes.UserId);
            if(userIdClaim == null)
                throw new ClaimNotFoundException(Principal.Constants.ApplicationClaimTypes.UserId);

            var identity = new IntelliFloClaimsIdentity(userIdClaim.Value, "Trusted");
            identity.AddClaims(claims);
            var principal = new IntelliFloClaimsPrincipal(identity);
            principal.AddIdentity(identity);

            Thread.CurrentPrincipal = principal;

            return new DisposableAction<IIntelliFloClaimsPrincipal>(ctx =>
            {
                Thread.CurrentPrincipal = previous;
            }, principal);
        }
    }
}
