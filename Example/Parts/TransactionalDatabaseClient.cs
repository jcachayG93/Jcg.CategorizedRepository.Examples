using Example.Database;
using Example.Types;
using Jcg.CategorizedRepository.Api;
using Jcg.CategorizedRepository.Api.DatabaseClient;
using Newtonsoft.Json;

namespace Example.Parts
{
    public class TransactionalDatabaseClient : ITransactionalDatabaseClient<
        CustomerDataModel, CustomerLookup>
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

        /// <inheritdoc />
        public Task UpsertAggregateAsync(string key, string eTag,
            CustomerDataModel aggregate,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                lock (LockObject)
                {
                    AddOperation(key, eTag, Clone(aggregate));
                }
            });
        }


        public Task<IETagDto<CategoryIndex<CustomerLookup>>?> GetCategoryIndex(
            string categoryKey, CancellationToken cancellationToken)
        {
            return Task.Run<IETagDto<CategoryIndex<CustomerLookup>>?>(() =>
            {
                lock (LockObject)
                {
                    var record = _database.GetData(categoryKey);

                    if (record is null)
                    {
                        return null;
                    }

                    var payload =
                        DeserializeOrThrow<CategoryIndex<CustomerLookup>>(
                            record.Payload);

                    payload = Clone(payload);

                    return new ETagDto<CategoryIndex<CustomerLookup>>(
                        record.Etag,
                        payload);
                }
            });
        }

        public Task UpsertCategoryIndex(string categoryKey, string eTag,
            CategoryIndex<CustomerLookup> categoryIndex,
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
                Id = input.Id,
                Name = input.Name,
                Orders = orders
            };
        }

        private CategoryIndex<CustomerLookup> Clone(
            CategoryIndex<CustomerLookup> input)
        {
            var lookups = input.Lookups
                .Select(l =>
                    new LookupDto<CustomerLookup>
                    {
                        Key = l.Key,
                        IsDeleted = l.IsDeleted,
                        DeletedTimeStamp = l.DeletedTimeStamp,
                        PayLoad = Clone(l.PayLoad)
                    }).ToArray();

            return new()
            {
                Lookups = lookups
            };
        }

        private CustomerLookup Clone(CustomerLookup input)
        {
            return new()
            {
                CustomerId = input.CustomerId,
                Name = input.Name,
                NumberOfOrders = input.NumberOfOrders
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