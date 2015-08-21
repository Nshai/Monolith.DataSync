using System.Diagnostics;
using System.Reflection;
using log4net;

namespace Microservice.Workflow.Engine
{
    public class Log4NetTraceListener : TraceListener
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            var logger = LogManager.GetLogger(source);
            switch (eventType)
            {
                case TraceEventType.Critical:
                    logger.Fatal(data);
                    break;
                case TraceEventType.Error:
                    logger.Error(data);
                    break;
                case TraceEventType.Information:
                    logger.Info(data);
                    break;
                case TraceEventType.Verbose:
                    logger.Debug(data);
                    break;
                case TraceEventType.Warning:
                    logger.Warn(data);
                    break;
                default:
                    base.TraceData(eventCache, source, eventType, id, data);
                    break;
            }
        }

        public override void Write(string message)
        {
            log.Info(message);
        }

        public override void WriteLine(string message)
        {
            log.Info(message);
        }
    }
}
