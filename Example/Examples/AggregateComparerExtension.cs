using Example.Domain;
using Example.Types;
using FluentAssertions;
using Jcg.CategorizedRepository.Api;

namespace Example.Examples
{
    public static class AggregateComparerExtension
    {
        public static void ShouldBeEquivalentTo(this Customer aggregate,
            Customer other)
        {
            var rootsMatch = aggregate.Id == other.Id &&
                             aggregate.Name == other.Name;

            aggregate.Orders.ShouldBeEquivalentTo(other.Orders, (x, y) =>
                x.Id == y.Id);
        }
    }

    public static class CollectionAssertions
    {
        public static void ShouldBeEquivalentTo<T1, T2>(
            this IEnumerable<T1> collection1, IEnumerable<T2> collection2,
            Func<T1, T2, bool> comparisonFunction)
        {
            if (collection1.Count() != collection2.Count())
            {
                Assert.Fail("Number of elements is different");
            }

            var elementsMatch = collection1.All(x =>
                                    collection2.Any(y =>
                                        comparisonFunction(x, y))) &&
                                collection2.All(x =>
                                    collection1.Any(y =>
                                        comparisonFunction(y, x)));

            if (!elementsMatch)
            {
                Assert.Fail("Elements do not match");
            }
        }
    }

    public static class LookupsVerifications
    {
        public static void ShouldContainOneWithId(
            this IEnumerable<LookupDto<CustomerLookup>> lookups,
            Guid customerId)
        {
            lookups.Any(l => l.PayLoad.CustomerId == customerId).Should()
                .BeTrue();
        }

        public static void ShouldNotContainOneWithId(
            this IEnumerable<LookupDto<CustomerLookup>> lookups,
            Guid customerId)
        {
            lookups.All(l => l.PayLoad.CustomerId != customerId).Should()
                .BeTrue();
        }
    }
}