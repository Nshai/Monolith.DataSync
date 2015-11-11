using System.Collections.Generic;
using FluentAssertions;
using Microservice.Workflow.Engine;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class GenerationTests
    {
        [Test]
        public void WhenPromoteToEmptyListThenAddedToGenerationZero()
        {
            var list = new GenerationList<int>();
            Promote(list, new[] { 1, 2, 3 }, 1);
            list.GetGeneration(0).Should().Contain(new[] { 1, 2, 3 });
            list.GetGeneration(1).Should().BeEmpty();
            list.GetGeneration(2).Should().BeEmpty();
            list.ToString().Should().Be("[1,2,3],[],[]");
        }

        [Test]
        public void WhenPromoteToListThenItemsPromotedToNextGeneration()
        {
            var list = new GenerationList<int>();
            Promote(list, new[] { 1, 2, 3 }, 2);
            list.GetGeneration(0).Should().BeEmpty();
            list.GetGeneration(1).Should().Contain(new[] { 1, 2, 3 });
            list.GetGeneration(2).Should().BeEmpty();
            list.ToString().Should().Be("[],[1,2,3],[]");
        }

        [Test]
        public void WhenPromoteToListThenItemsPromotedToLastGeneration()
        {
            var list = new GenerationList<int>();
            Promote(list, new[] { 1, 2, 3 }, 3);
            list.GetGeneration(0).Should().BeEmpty();
            list.GetGeneration(1).Should().BeEmpty();
            list.GetGeneration(2).Should().Contain(new[] { 1, 2, 3 });
            list.ToString().Should().Be("[],[],[1,2,3]");
        }

        [Test]
        public void WhenPromoteToListThenItemsNotPromotedPastLastGeneration()
        {
            var list = new GenerationList<int>();
            Promote(list, new[] { 1, 2, 3 }, 4);
            list.GetGeneration(2).Should().Contain(new[] { 1, 2, 3 });
            list.ToString().Should().Be("[],[],[1,2,3]");
        }

        [Test]
        public void WhenPromotePartialListThenItemsPromotedCorrectly()
        {
            var list = new GenerationList<int>();
            list.Promote(new[] { 1, 2, 3 });
            list.Promote(new[] { 1, 2, 4 });
            list.ToString().Should().Be("[4],[1,2],[]");
            list.Promote(new[] { 1, 4, 5 });
            list.ToString().Should().Be("[5],[4],[1]");
        }

        [Test]
        public void WhenPromoteNoItemsToListThenListIsEmptied()
        {
            var list = new GenerationList<int>();
            list.Promote(new[] { 1, 2, 4 });
            list.Promote(new int[0]);
            list.GetGeneration(0).Should().BeEmpty();
            list.GetGeneration(1).Should().BeEmpty();
            list.GetGeneration(2).Should().BeEmpty();
            list.ToString().Should().Be("[],[],[]");
        }

        private void Promote(GenerationList<int> list, IEnumerable<int> ids, int promoteCount)
        {
            for (int i = 0; i < promoteCount; i++)
            {
                list.Promote(ids);
            }
        }
    }
}