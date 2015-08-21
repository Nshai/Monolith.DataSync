using System;

namespace Microservice.Workflow.v1.Activities
{
    public class DayDelayPeriod : IDelayPeriod
    {
        public TimeSpan GetPeriod(int count)
        {
            return TimeSpan.FromDays(count);
        }
    }
}