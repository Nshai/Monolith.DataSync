using System;
using System.Collections.Generic;
using System.Globalization;
using Microservice.Workflow.v1.Activities;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class DateCalculatorTests
    {
        private const string DateFormat = "yyyy-MM-dd";
        private readonly Func<DateTime, DateTime, IEnumerable<DateTime>> getHolidays;


        public DateCalculatorTests()
        {
            getHolidays = (sd, ed) =>
            {
                var holidaysDates = new List<DateTime>();
                var date = sd;
                while (date <= ed)
                {
                    if (date == new DateTime(2015, 7, 14))
                        holidaysDates.Add(date);
                    date = date.AddDays(1);
                }
                return holidaysDates;
            };
        }

        [TestCase("2015-07-09", 0, false, ExpectedResult = "2015-07-09")]
        [TestCase("2015-07-09", 1, false, ExpectedResult = "2015-07-10")]
        [TestCase("2015-07-09", -1, false, ExpectedResult = "2015-07-08")]
        [TestCase("2015-07-09", 2, false, ExpectedResult = "2015-07-11")]
        [TestCase("2015-07-09", 3, false,  ExpectedResult = "2015-07-12")]
        [TestCase("2015-07-09", 2, true, ExpectedResult = "2015-07-13")]
        [TestCase("2015-07-09", 3, true, ExpectedResult = "2015-07-15")]
        public string WhenDate(string dateTime, int delayDays, bool businessDaysOnly)
        {
            var date = DateTime.ParseExact(dateTime, DateFormat, CultureInfo.InvariantCulture);
            var calculatedDate = DateCalculator.AddDays(date, TimeSpan.FromDays(delayDays), businessDaysOnly, getHolidays);
            return calculatedDate.ToString(DateFormat);
        }
    }
}
