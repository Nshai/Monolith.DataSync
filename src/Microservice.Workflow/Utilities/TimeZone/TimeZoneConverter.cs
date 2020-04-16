using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Workflow.Utilities.TimeZone
{
    public class TimeZoneConverter : ITimeZoneConverter
    {
        private readonly IDateTimeZoneProvider timeZoneProvider;
        public TimeZoneConverter(IDateTimeZoneProvider timeZoneProvider)
        {
            this.timeZoneProvider = timeZoneProvider ?? throw new ArgumentNullException(nameof(timeZoneProvider));
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
