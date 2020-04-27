using NodaTime;
using System;

namespace Microservice.Workflow.Utilities.TimeZone
{
    public class TimeZoneConverter : ITimeZoneConverter
    {
        private readonly IDateTimeZoneProvider timeZoneProvider;
        public TimeZoneConverter(IDateTimeZoneProvider timeZoneProvider)
        {
            if (timeZoneProvider == null)
                throw new ArgumentNullException("timeZoneProvider");

            this.timeZoneProvider = timeZoneProvider;
        }

        public DateTime ConvertFromUtc(DateTime value, string targetTimeZone)
        {
            if (targetTimeZone == null)
            {
                throw new InvalidOperationException("User timezone not set.");
            }

            var userTimeZone = timeZoneProvider[targetTimeZone];
            var rawDateTime = LocalDateTime.FromDateTime(value);
            var instantDateTime = rawDateTime.InUtc().ToInstant();
            var zonedDateTime = new ZonedDateTime(instantDateTime, userTimeZone);
            return zonedDateTime.ToDateTimeUnspecified();
        }
    }
}
