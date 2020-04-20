using System;

namespace Microservice.Workflow.Utilities.TimeZone
{
    public interface ITimeZoneConverter
    {
        DateTime ConvertFromUtc(DateTime value, string targetTimeZone);
    }
}
