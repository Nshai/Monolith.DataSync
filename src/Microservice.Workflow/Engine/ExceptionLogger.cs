using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Principal;
using log4net;

namespace Microservice.Workflow.Engine
{
    internal static class ExceptionLogger
    {
        private const string ErrorId = "ErrorId";
        private const string Version = "Version";
        private const string UserId = "UserId";
        private const string TenantId = "TenantId";

        internal static string Log(Exception ex)
        {
            var logger = LogManager.GetLogger(typeof(ExceptionLogger));

            var errorId = ErrorIdGenerator.GeneratorErrorId();
            var contextInfo = GetExceptionInfo(errorId);
            var errorData = contextInfo.Keys.Aggregate(string.Empty, (current, contextInfoKey) => string.Format("{0}\n{1}={2}", current, contextInfoKey, contextInfo[contextInfoKey]));

            logger.Error(errorData, ex);
            return errorId;
        }

        private static Dictionary<string, string> GetExceptionInfo(string errorId)
        {
            var contextInfo = new Dictionary<string, string> {{ErrorId, errorId}, {Version, Assembly.GetExecutingAssembly().GetName().Version.ToString()}};
            var userContext = Thread.CurrentPrincipal.AsIFloPrincipal();

            if (userContext != null)
            {
                contextInfo.Add(UserId, Convert.ToString(userContext.UserId));
                contextInfo.Add(TenantId, Convert.ToString(userContext.TenantId));
            }
            else
            {
                contextInfo.Add(UserId, "None");
                contextInfo.Add(TenantId, "None");
            }

            return contextInfo;
        }
    }
}