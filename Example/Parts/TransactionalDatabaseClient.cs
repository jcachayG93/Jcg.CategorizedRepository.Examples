using Example.Database;
using Example.Types;
using Jcg.CategorizedRepository.Api;
using Newtonsoft.Json;

namespace Example.Parts
{
    public class TransactionalDatabaseClient : ITransactionalDatabaseClient<
        CustomerDataModel, Lookup>
    {
        public TransactionalDatabaseClient(
            IInMemoryDatabase database)
        {
            _database = database;
        }

        public Task<IETagDto<CustomerDataModel>?> GetAggregateAsync(string key,
            CancellationToken cancellationToken)
        {
            return Task.Run<IETagDto<CustomerDataModel>?>(() =>
            {
                lock (LockObject)
                {
                    var record = _database.GetData(key);

                    if (record is null)
                    {
                        return null;
                    }

                    var payload =
                        DeserializeOrThrow<CustomerDataModel>(record.Payload);

                    payload = Clone(payload);

                    return new ETagDto<CustomerDataModel>(record.Etag, payload);
                }
            });
        }

        public Task UpsertAggregateAsync(string eTag,
            CustomerDataModel aggregate, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                lock (LockObject)
                {
                    AddOperation(aggregate.Key, eTag, Clone(aggregate));
                }
            });
        }

        public Task<IETagDto<CategoryIndex<Lookup>>?> GetCategoryIndex(
            string categoryKey, CancellationToken cancellationToken)
        {
            return Task.Run<IETagDto<CategoryIndex<Lookup>>?>(() =>
            {
                lock (LockObject)
                {
                    var record = _database.GetData(categoryKey);

                    if (record is null)
                    {
                        return null;
                    }

                    var payload =
                        DeserializeOrThrow<CategoryIndex<Lookup>>(
                            record.Payload);

                    payload = Clone(payload);

                    return new ETagDto<CategoryIndex<Lookup>>(record.Etag,
                        payload);
                }
            });
        }

        public Task UpsertCategoryIndex(string categoryKey, string eTag,
            CategoryIndex<Lookup> categoryIndex,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                lock (LockObject)
                {
                    AddOperation(categoryKey, eTag, Clone(categoryIndex));
                }
            });
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                lock (LockObject)
                {
                    _database.UpsertAndCommit(_operations.Select(o => o.Value)
                        .ToArray());
                }
            });
        }


        private void AddOperation(string key, string etag,
            object data)
        {
            var payload = JsonConvert.SerializeObject(data);

            var record = new DatabaseRecord(etag, payload);

            var operation = new UpsertOperation(key, record);

            if (!_operations.TryAdd(key, operation))
            {
                _operations[key] = operation;
            }
        }

        private T DeserializeOrThrow<T>(string payload)
        {
            var result = JsonConvert.DeserializeObject<T>(payload);

            if (result is null)
            {
                throw new Exception("Cant deserialize");
            }

            return result;
        }

        private CustomerDataModel Clone(CustomerDataModel input)
        {
            var orders = input.Orders.Select(o =>
                new OrderDataModel()
                {
                    Id = o.Id
                }).ToList();

            return new()
            {
                Key = input.Key,
                Id = input.Id,
                Name = input.Name,
                Orders = orders
            };
        }

        private CategoryIndex<Lookup> Clone(CategoryIndex<Lookup> input)
        {
            var lookups = input.Lookups.Select(l =>
                new Lookup
                {
                    CustomerId = l.CustomerId,
                    Name = l.Name,
                    NumberOfOrders = l.NumberOfOrders,
                    Key = l.Key,
                    IsDeleted = l.IsDeleted,
                    DeletedTimeStamp = l.DeletedTimeStamp
                });

            return new()
            {
                Lookups = lookups
            };
        }

        private static readonly object LockObject = new();
        private readonly IInMemoryDatabase _database;

        private readonly Dictionary<string, UpsertOperation>
            _operations = new();

        private class ETagDto<T> : IETagDto<T>
        {
            public ETagDto(string etag, T payload)
            {
                Etag = etag;
                Payload = payload;
            }

            public string Etag { get; }
            public T Payload { get; }
        }
    }
}