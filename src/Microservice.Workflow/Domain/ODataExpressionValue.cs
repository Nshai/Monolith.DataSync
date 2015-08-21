using System;
using System.Text.RegularExpressions;

namespace Microservice.Workflow.Domain
{
    public class ODataExpressionValue
    {
        public string Value { get; set; }

        public ODataExpressionValue(object value)
        {
            Value = FormatValue(value);
        }

        /// <summary>
        /// Escapes special characters for odata expression
        /// </summary>
        /// <param name="value">string value</param>
        /// <returns>string escaped value</returns>
        private static string EscapeSingleQuotes(string value)
        {
            // http://stackoverflow.com/questions/3979367/how-to-escape-a-single-quote-to-be-used-in-an-odata-query
            return Regex.Replace(value, @"[\']", @"'$0");
        }

        /// <summary>
        /// Formats a given filter value as expected by odata convention
        /// </summary>
        /// <param name="value">operand filter value</param>
        /// <returns>formated string value</returns>
        private static string FormatValue(object value)
        {
            if (value == null) return null;

            if (value is string) return string.Format("'{0}'", EscapeSingleQuotes(value.ToString()));
            if (value.IsNumeric()) return value.ToString();
            if (value is Boolean) return value.ToString().ToLower();
            if (value is DateTime) return string.Format("datetime'{0}'", ((DateTime)value).ToString("s")); //datetime'2010-01-25T02:13:40.1374695Z'

            throw new FormatException("Invalid value type, only supports string, boolean, numeric and date time type");
        }

        public override string ToString()
        {
            return Value;
        }
    }
}