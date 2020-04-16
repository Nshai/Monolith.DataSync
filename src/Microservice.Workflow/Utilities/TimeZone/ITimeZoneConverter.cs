using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Workflow.Utilities.TimeZone
{
    public interface ITimeZoneConverter
    {
        DateTime ConvertFromUtc(DateTime value, string targetTimeZone);
    }
}
