using Microservice.Workflow.Utilities.TimeZone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Workflow.Tests
{
    public class SimpleTimeZoneConverter : ITimeZoneConverter
    {
        public DateTime ConvertFromUtc(DateTime value, string targetTimeZone)
        {
            return value.AddHours(1);
        }
    }
}
