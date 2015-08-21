using System;
using System.Security.Principal;
using System.Threading;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Principal;

namespace Microservice.Workflow
{
    public static class PrincipalExtensions
    {
        public static DisposableAction AsDelegate(this IPrincipal principal, Func<IPrincipal> getContext)
        {
            var previous = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = new IntelliFloClaimsPrincipal(getContext());

            return new DisposableAction(() => Thread.CurrentPrincipal = previous);
        }
    }
}
