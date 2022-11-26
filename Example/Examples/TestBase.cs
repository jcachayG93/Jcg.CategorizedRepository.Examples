using Example.Database;
using Example.Domain;
using Example.Parts;
using Example.Types;
using Jcg.CategorizedRepository.Api;
using Jcg.CategorizedRepository.Api.DatabaseClient;

namespace Example.Examples;

public abstract class TestBase
{
    protected TestBase()
    {
        var db = new InMemoryDatabase();

        _dbClient = new TransactionalDatabaseClient(db);

        _categoryKey = new(Guid.NewGuid());

        Key = new RepositoryIdentity(Guid.NewGuid());
    }

    protected RepositoryIdentity Key { get; }

    protected ICategorizedRepository<Customer, CustomerLookup> CreateSut()
    {
        var aggregateMapper = new AggregateMapper();
        var lookupMapper = new AggregateToLookupMapper();

        return CategorizedRepositoryFactory.Create(
            _categoryKey,
            _dbClient,
            aggregateMapper,
            lookupMapper);
    }

    protected async Task InitializeCategoryIndexInDatabase()
    {
        var sut = CreateSut();

        await sut.InitializeCategoryIndexAsync(CancellationToken.None);

        await sut.CommitChangesAsync(CancellationToken.None);
    }

    protected Customer RandomCustomerWithOrders(int numberOfOrders = 0)
    {
        var result = new Customer(Guid.NewGuid(), Guid.NewGuid().ToString());

        for (var i = 0; i < numberOfOrders; i++)
        {
            result.AddOrder(Guid.NewGuid());
        }

        return result;
    }

    private readonly RepositoryIdentity _categoryKey;

    private readonly ITransactionalDatabaseClient<CustomerDataModel,
            CustomerLookup>
        _dbClient;
}