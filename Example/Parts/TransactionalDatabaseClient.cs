using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Example.Database;
using Example.Domain;
using Example.Types;
using Jcg.CategorizedRepository.Api;
using Newtonsoft.Json;

namespace Example.Parts
{
    public class TransactionalDatabaseClient : ITransactionalDatabaseClient<CustomerDataModel, Lookup>
    {
        private readonly IInMemoryDatabase _database;
        private static readonly object LockObject = new();

        public TransactionalDatabaseClient(
            IInMemoryDatabase database)
        {
            _database = database;
        }
        public Task<IETagDto<CustomerDataModel>?> GetAggregateAsync(string key, CancellationToken cancellationToken)
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

                        var payload = DeserializeOrThrow<CustomerDataModel>(record.Payload);

                        payload = Clone(payload);

                        return new ETagDto<CustomerDataModel>(record.Etag, payload);
                    }
                });
            
        }

        public Task UpsertAggregateAsync(string eTag, CustomerDataModel aggregate, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                lock (LockObject)
                {
                    var operation = new UpsertOperation(aggregate.Key,)
                }
            })
        }

        public Task<IETagDto<CategoryIndex<Lookup>>?> GetCategoryIndex(string categoryKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpsertCategoryIndex(string categoryKey, string eTag, CategoryIndex<Lookup> categoryIndex,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private void AddOperation(string key, UpsertOperation operation)
        {
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
            var orders = input.Orders.Select(o=>
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

            return new() {Lookups = lookups};
        }

        private readonly Dictionary<string, UpsertOperation> _operations = new();

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
