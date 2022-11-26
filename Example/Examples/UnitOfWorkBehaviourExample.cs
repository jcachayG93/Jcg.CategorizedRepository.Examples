using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Example.Domain;
using Example.Types;
using FluentAssertions;
using Jcg.CategorizedRepository.Api;

namespace Example.Examples
{
    public class UnitOfWorkBehaviourExample : TestBase
    {
        [Fact]
        public async Task EachCategorizedRepositoryHasItsOwnVersionOfData()
        {
            // ************ ARRANGE ************

            await InitializeCategoryIndexInDatabase();

            AddRandomAggregateToDatabase(out RepositoryIdentity key, out Customer aggregate);

            ICategorizedRepository<Customer, CustomerLookup> sut1 = CreateSut();

            ICategorizedRepository<Customer, CustomerLookup> sut2 = CreateSut();

            // ************ ACT && Assert ****************

            var aggregate1 = (await sut1.GetAggregateAsync(key, CancellationToken.None))!;

            var aggregate2 = (await sut2.GetAggregateAsync(key, CancellationToken.None))!;

          aggregate1.ShouldBeEquivalentTo(aggregate2);

            aggregate1.UpdateName("John Doe");

            await sut1.UpsertAsync(key, aggregate1, CancellationToken.None);

            aggregate1 = (await sut1.GetAggregateAsync(key, CancellationToken.None))!;

            aggregate2 = (await sut2.GetAggregateAsync(key, CancellationToken.None))!;

            aggregate1.ShouldBeDifferentThan(aggregate2);

        }

        [Fact]
        public async Task CommitChanges_AppliesDataFromTheUnitOfWorkToTheDatabase()
        {
            // ************ ARRANGE ************

            await InitializeCategoryIndexInDatabase();

            AddRandomAggregateToDatabase(out var key, out var aggregate);

            var sut1 = CreateSut();

            var aggregate1 = await sut1.GetAggregateAsync(key, CancellationToken.None);

            aggregate1.UpdateName("Jane Doe");

            await sut1.UpsertAsync(key, aggregate1, CancellationToken.None);

            // ************ ACT ****************

            await sut1.CommitChangesAsync(CancellationToken.None);

            // ************ ASSERT *************

            var sut2 = CreateSut();

            var restoredAggregate = await sut2.GetAggregateAsync(key, CancellationToken.None);

            restoredAggregate!.Name.Should().Be("Jane Doe");
        }
    }
}
