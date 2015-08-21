using System;

namespace Microservice.Workflow.v1.Activities
{
    public class MinuteDelayPeriod : IDelayPeriod
    {
        public TimeSpan GetPeriod(int count)
        {
            return TimeSpan.FromMinutes(count);
        }
    }
}