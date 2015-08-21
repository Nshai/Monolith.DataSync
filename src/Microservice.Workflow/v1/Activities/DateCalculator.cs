using System;
using System.Collections.Generic;
using System.Linq;

namespace Microservice.Workflow.v1.Activities
{
    public static class DateCalculator
    {
        public static DateTime AddDays(DateTime startDate, TimeSpan delay, bool businessDays = false, Func<DateTime, DateTime, IEnumerable<DateTime>> getBankHolidays = null)
        {
            var includedBankHolidays = 0;
            while (true)
            {
                if (!businessDays) return startDate.Add(delay);
                var days = (int)delay.TotalDays;
                var fractionOfDay = delay - new TimeSpan(days, 0, 0, 0);

                var dayOfWeek = (int)startDate.DayOfWeek;
                var temp = days + dayOfWeek + 1;
                if (dayOfWeek != 0) temp--;

                // Skip weekends
                var endDate = startDate.AddDays((Math.Floor((double)temp / 5) * 2) - dayOfWeek + temp - (2 * Convert.ToInt32(temp % 5 == 0)));
                if (getBankHolidays == null) return endDate.Add(fractionOfDay);

                // Add any bank holidays but exclude ones we've already accounted for
                var bankHolidays = getBankHolidays(startDate, endDate).Count(d => d >= startDate && d <= endDate);
                var remainingBankHoliday = bankHolidays - includedBankHolidays;
                if (remainingBankHoliday <= 0) return endDate.Add(fractionOfDay);
                delay = delay.Add(new TimeSpan(remainingBankHoliday, 0, 0, 0));
                includedBankHolidays += remainingBankHoliday;
            }
        }
    }
}
