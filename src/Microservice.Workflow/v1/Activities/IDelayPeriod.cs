using System;

namespace Microservice.Workflow.v1.Activities
{
    public interface IDelayPeriod
    {
        TimeSpan GetPeriod(int count);
    }
}