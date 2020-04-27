using FluentAssertions;
using Microservice.Workflow.Modules;
using Microservice.Workflow.Utilities.TimeZone;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class TimeZoneConverterTests
    {
        private TimeZoneConverter underTest;

        [SetUp]
        public void SetUp()
        {
            underTest = new TimeZoneConverter(WorkflowAutofacModule.BuildDateTimeZoneProvider());
        }

        [Test]
        [TestCaseSource(nameof(ConvertFromUtc_DataSource))]
        public void Test_ConvertFromUtc_When_Valid_Input_Should_Be_Valid_Output(
            string timeZone, DateTime input, DateTime expectedResult)
        {

            var actualResult = underTest.ConvertFromUtc(input, timeZone);
            actualResult.Should().Be(expectedResult);
        }

        public static IEnumerable<object> ConvertFromUtc_DataSource()
        {
            //Etc/UTC
            //winter
            yield return ToTestCaseData("Etc/UTC",
                new DateTime(2019, 12, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 12, 14, 1, 2, 3, DateTimeKind.Unspecified));
            //summer
            yield return ToTestCaseData("Etc/UTC",
                new DateTime(2019, 6, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 6, 14, 1, 2, 3, DateTimeKind.Unspecified));
            //Europe/London
            //winter
            yield return ToTestCaseData("Europe/London",
                new DateTime(2019, 12, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 12, 14, 1, 2, 3, DateTimeKind.Unspecified));
            //summer
            yield return ToTestCaseData("Europe/London",
                new DateTime(2019, 6, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 6, 14, 2, 2, 3, DateTimeKind.Unspecified));
            //Australia/Perth
            //winter
            yield return ToTestCaseData("Australia/Perth",
                new DateTime(2019, 12, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 12, 14, 9, 2, 3, DateTimeKind.Unspecified));
            //summer
            yield return ToTestCaseData("Australia/Perth",
                new DateTime(2019, 6, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 6, 14, 9, 2, 3, DateTimeKind.Unspecified));
            //Australia/Sydney
            //winter
            yield return ToTestCaseData("Australia/Sydney",
                new DateTime(2019, 12, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 12, 14, 12, 2, 3, DateTimeKind.Unspecified));
            //summer
            yield return ToTestCaseData("Australia/Sydney",
                new DateTime(2019, 6, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 6, 14, 11, 2, 3, DateTimeKind.Unspecified));
            //Europe/Minsk
            //winter
            yield return ToTestCaseData("Europe/Minsk",
                new DateTime(2019, 12, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 12, 14, 4, 2, 3, DateTimeKind.Unspecified));
            //summer
            yield return ToTestCaseData("Europe/Minsk",
                new DateTime(2019, 6, 14, 1, 2, 3, DateTimeKind.Unspecified),
                new DateTime(2019, 6, 14, 4, 2, 3, DateTimeKind.Unspecified));
        }

        private static object ToTestCaseData(string timezone, DateTime input, DateTime expectedResult)
            => new TestCaseData(timezone, input, expectedResult);
    }
}
