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
            AreEquivalent(aggregate, other).Should().BeTrue();
        }

        public static void ShouldBeDifferentThan(this Customer aggregate,
            Customer other)
        {
            AreEquivalent(aggregate, other).Should().BeFalse();
        }

        private static bool AreEquivalent(Customer aggregate1, Customer aggregate2)
        {
            var rootsMatch = aggregate1.Id == aggregate2.Id &&
                             aggregate1.Name == aggregate2.Name;

            if (!rootsMatch)
            {
                return false;
            }

            var ordersMatch = aggregate1.Orders.IsEquivalentTo(aggregate2.Orders, (x, y) => x.Id == y.Id);

            return ordersMatch;
        }



    }

    public static class CollectionAssertions
    {
        public static void ShouldBeEquivalentTo<T1, T2>(
            this IEnumerable<T1> collection1, IEnumerable<T2> collection2,
            Func<T1, T2, bool> comparisonFunction)
        {
            collection1.IsEquivalentTo(collection2, comparisonFunction).Should().BeTrue();
        }

        public static bool IsEquivalentTo<T1, T2>(
            this IEnumerable<T1> collection1, IEnumerable<T2> collection2,
            Func<T1, T2, bool> comparisonFunction)
        {
            if (collection1.Count() != collection2.Count())
            {
                return false;
            }

            var elementsMatch = collection1.All(x =>
                                    collection2.Any(y =>
                                        comparisonFunction(x, y))) &&
                                collection2.All(x =>
                                    collection1.Any(y =>
                                        comparisonFunction(y, x)));

            if (!elementsMatch)
            {
                return false;
            }

            return true;
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