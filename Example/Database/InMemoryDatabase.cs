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
            if (operations.Any(EtagMismatch))
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

        private bool EtagMismatch(UpsertOperation operation)
        {
            if (string.IsNullOrEmpty(operation.Data.Etag))
            {
                return false;
            }
            if (_data.TryGetValue(operation.Key, out var record))
            {
                if (record.Etag != operation.Data.Etag)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyChanges(UpsertOperation[] operations)
        {
            foreach (var op in operations)
            {
                if (!_data.TryAdd(op.Key, op.Data))
                {
                    _data[op.Key] = op.Data;
                }
            }
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
