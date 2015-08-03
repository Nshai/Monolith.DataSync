using System;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public class DayDelayPeriod : IDelayPeriod
    {
        public TimeSpan GetPeriod(int count)
        {
            return TimeSpan.FromDays(count);
        }
    }
}