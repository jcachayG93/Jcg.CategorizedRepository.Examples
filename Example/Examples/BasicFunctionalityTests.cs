using FluentAssertions;
using Jcg.CategorizedRepository.Api;
using Jcg.CategorizedRepository.Api.Exceptions;

namespace Example.Examples
{
    public class BasicFunctionalityTests : TestBase
    {
        [Fact]
        public async Task Initialize_A_Category()
        {
            // ************ ARRANGE ************

            var sut = CreateSut();

            // ************ ACT ****************

            await sut.InitializeCategoryIndexAsync(CancellationToken.None);

            // ************ ASSERT *************

            var result =
                await sut.LookupNonDeletedAsync(CancellationToken.None);

            result.Should().NotBeNull();
        }


        [Fact]
        public async Task InitializeCategory_ThatIsAlreadyInitialized_Throws()
        {
            // ************ ARRANGE ************

            var sut = CreateSut();

            await sut.InitializeCategoryIndexAsync(CancellationToken.None);

            // ************ ACT ****************

            Func<Task> fun = async () =>
            {
                await sut.InitializeCategoryIndexAsync(CancellationToken
                    .None);
            };

            // ************ ASSERT *************

            await fun.Should()
                .ThrowAsync<CategoryIndexIsAlreadyInitializedException>();
        }


        [Fact]
        public async Task Upsert_WhenCategoryIsUninitialized_Throws()
        {
            // ************ ARRANGE ************

            var sut = CreateSut();

            // ************ ACT ****************

            Func<Task> fun = async () =>
            {
                await sut.UpsertAsync(new RepositoryIdentity(Guid.NewGuid()),
                    RandomCustomerWithOrders(), CancellationToken.None);
            };

            // ************ ASSERT *************

            await fun.Should()
                .ThrowAsync<CategoryIndexIsUninitializedException>();
        }


        [Fact]
        public async Task UpsertAggregateExample()
        {
            // ************ ARRANGE ************

            var sut = CreateSut();

            await sut.InitializeCategoryIndexAsync(CancellationToken.None);

            var aggregate = RandomCustomerWithOrders(10);

            // ************ ACT ****************

            await sut.UpsertAsync(Key, aggregate, CancellationToken.None);

            // ************ ASSERT *************

            var lookups =
                await sut.LookupNonDeletedAsync(CancellationToken.None);

            lookups.Any(l =>
                    l.PayLoad.CustomerId == aggregate.Id &&
                    l.PayLoad.Name == aggregate.Name &&
                    l.PayLoad.NumberOfOrders == 10)
                .Should().BeTrue();

            var restoredAggregate =
                await sut.GetAggregateAsync(Key, CancellationToken.None);

            restoredAggregate.ShouldBeEquivalentTo(aggregate);
        }


        [Fact]
        public async Task DeleteAndRestore()
        {
            // ************ ARRANGE ************

            var sut = CreateSut();

            await sut.InitializeCategoryIndexAsync(CancellationToken.None);

            var aggregate = RandomCustomerWithOrders();

            await sut.UpsertAsync(Key, aggregate, CancellationToken.None);

            // ************ ACT && ASSERT ****************

            (await sut.LookupNonDeletedAsync(CancellationToken.None))
                .ShouldContainOneWithId(aggregate.Id);

            // Delete
            await sut.DeleteAsync(Key, CancellationToken.None);

            // Now the lookup is in the Deleted colletion
            (await sut.LookupNonDeletedAsync(CancellationToken.None))
                .ShouldNotContainOneWithId(aggregate.Id);
            (await sut.LookupDeletedAsync(CancellationToken.None))
                .ShouldContainOneWithId(aggregate.Id);

            // Restore
            await sut.RestoreAsync(Key, CancellationToken.None);

            // Now the lookup is in the NON Deleted colletion
            (await sut.LookupNonDeletedAsync(CancellationToken.None))
                .ShouldContainOneWithId(aggregate.Id);
            (await sut.LookupDeletedAsync(CancellationToken.None))
                .ShouldNotContainOneWithId(aggregate.Id);
        }

        [Fact]
        public async Task GetAggregate_AggregateIsDeleted_StillWorks()
        {
            // ************ ARRANGE ************

            var sut = CreateSut();

            await sut.InitializeCategoryIndexAsync(CancellationToken.None);

            var aggregate = RandomCustomerWithOrders();

            await sut.UpsertAsync(Key, aggregate, CancellationToken.None);

            // ************ ACT ****************

            await sut.DeleteAsync(Key, CancellationToken.None);

            var restoredAggregate = await sut.GetAggregateAsync(Key, CancellationToken.None);

            // ************ ASSERT *************

            restoredAggregate.ShouldBeEquivalentTo(aggregate);
        }

        [Fact]
        public async Task CommitWasNotCalled_ExistInLocalCacheNotDatabase()
        {
            // ************ ARRANGE ************

            await InitializeCategoryIndexInDatabase();

            var sut1 = CreateSut();

            var sut2 = CreateSut();

            var aggregate = RandomCustomerWithOrders();

            // ************ ACT ****************

            await sut1.UpsertAsync(Key, aggregate, CancellationToken.None);

            var restoredAggregateFromSut1 = await sut1.GetAggregateAsync(Key, CancellationToken.None);
            var restoredAggregateFromSut2 = await sut2.GetAggregateAsync(Key, CancellationToken.None);

            // ************ ASSERT *************

            restoredAggregateFromSut1.Should().NotBeNull();
            restoredAggregateFromSut2.Should().BeNull();
        }

        [Fact]
        public async Task CommitWasCalled_UpdatesDatabase()
        {
            // ************ ARRANGE ************

            await InitializeCategoryIndexInDatabase();

            var sut1 = CreateSut();

            var aggregate = RandomCustomerWithOrders();

            await sut1.UpsertAsync(Key, aggregate, CancellationToken.None);

            // ************ ACT ****************

            await sut1.CommitChangesAsync(CancellationToken.None);

            // ************ ASSERT *************

            var sut2 = CreateSut();

            var restoredAggregate = await sut2.GetAggregateAsync(Key, CancellationToken.None);

            restoredAggregate.ShouldBeEquivalentTo(aggregate);
        }
    }
}