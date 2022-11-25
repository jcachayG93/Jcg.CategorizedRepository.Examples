using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Example.Database
{
    public class InMemoryDatabase : IInMemoryDatabase
    {
        public DatabaseRecord? GetData(string key)
        {
            _data.TryGetValue(key, out var result);

            return result;
        }
        public void UpsertAndCommit(UpsertOperation[] operations)
        {
            AssertETagMatch(operations);

            operations = EvolveTags(operations);

            ApplyChanges(operations);
        }

        
        private void AssertETagMatch(UpsertOperation[] operations)
        {
            if (operations.Any(EtagMatch))
            {
                throw new OptimisticConcurrencyException();
            }
        }

        private UpsertOperation[] EvolveTags(UpsertOperation[] operations)
        {
            return operations.Select(EvolveTag).ToArray();
        }

        private UpsertOperation EvolveTag(UpsertOperation operation)
        {
            var data = operation.Data with {Etag = Guid.NewGuid().ToString()};

            return operation with {Data = data};
        }

        private bool EtagMatch(UpsertOperation operation)
        {
            if (string.IsNullOrEmpty(operation.Data.Etag))
            {
                return true;
            }
            if (_data.TryGetValue(operation.Key, out var record))
            {
                if (record.Etag != operation.Data.Etag)
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplyChanges(UpsertOperation[] operations)
        {
            throw new NotImplementedException();
        }

        private void ApplyChanges(UpsertOperation operation)
        {
            if (!_data.TryAdd(operation.Key, operation.Data))
            {
                _data[operation.Key] = operation.Data;
            }
        }

        private Dictionary<string, DatabaseRecord> _data = new();

    }
}
