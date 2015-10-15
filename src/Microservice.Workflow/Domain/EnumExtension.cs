using System;
using System.Text.RegularExpressions;

namespace Microservice.Workflow.Domain
{
    /// <summary>
    /// Enum extension methods
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Split enum value on capital letters
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string ToPrettyString(this Enum @enum)
        {
            var splitStatus = Regex.Split(@enum.ToString(), @"(?<!^)(?=[A-Z])");
            return string.Join(" ", splitStatus);
        }
    }
}