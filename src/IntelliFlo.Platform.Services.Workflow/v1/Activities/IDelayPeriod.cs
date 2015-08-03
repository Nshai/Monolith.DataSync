using System;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public interface IDelayPeriod
    {
        TimeSpan GetPeriod(int count);
    }
}