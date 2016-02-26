using FluentAssertions;
using Microservice.Workflow.Engine;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class CounterTests
    {
        [Test]
        public void When_Decrement_At_Zero_Then_Counter_Returns_Zero()
        {
            var counter = new Counter();
            counter.Decrement();
            counter.Value.Should().Be(0);
        }

        [Test]
        public void When_Increment_Then_Counter_Returns_Correctly()
        {
            var counter = new Counter(5);
            counter.Increment();
            counter.Value.Should().Be(6);
            counter.Increment();
            counter.Value.Should().Be(7);
        }

        [Test]
        public void When_Decrement_Then_Counter_Returns_Correctly()
        {
            var counter = new Counter(5);
            counter.Decrement();
            counter.Value.Should().Be(4);
            counter.Decrement();
            counter.Value.Should().Be(3);
        }
    }
}
