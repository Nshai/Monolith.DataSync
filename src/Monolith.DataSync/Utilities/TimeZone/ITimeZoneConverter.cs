using System;

namespace Microservice.DataSync.Utilities.TimeZone
{
    public interface ITimeZoneConverter
    {
        DateTime ConvertFromUtc(DateTime value, string targetTimeZone);
    }
}
