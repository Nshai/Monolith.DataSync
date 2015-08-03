using System;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public class MinuteDelayPeriod : IDelayPeriod
    {
        public TimeSpan GetPeriod(int count)
        {
            return TimeSpan.FromMinutes(count);
        }
    }
}