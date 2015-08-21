using System.Linq;
using System.Threading;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Identity;
using IntelliFlo.Platform.Principal;
using Constants = Microservice.Workflow.Engine.Constants;

namespace Microservice.Workflow.v1.Activities
{
    public static class UserContextBuilder
    {
        public static DisposableAction<IIntelliFloClaimsPrincipal> FromBearerToken(string token)
        {
            var previous = Thread.CurrentPrincipal;

            var trustedClientAuthentication = IoC.Resolve<ITrustedClientAuthenticationScheme>(Constants.ContainerId);

            var claims = trustedClientAuthentication.Validate(token).Result;
            var userIdClaim = claims.SingleOrDefault(c => c.Type == IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.UserId);
            if(userIdClaim == null)
                throw new ClaimNotFoundException(IntelliFlo.Platform.Principal.Constants.ApplicationClaimTypes.UserId);

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
