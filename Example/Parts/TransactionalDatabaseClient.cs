using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Example.Database;
using Example.Types;
using Jcg.CategorizedRepository.Api;

namespace Example.Parts
{
    public class TransactionalDatabaseClient : ITransactionalDatabaseClient<CustomerDataModel, LookupDataModel>
    {
        private static readonly object LockObject = new();

        public TransactionalDatabaseClient(
            IInMemoryDatabase database)
        {
            
        }
        public Task<IETagDto<CustomerDataModel>?> GetAggregateAsync(string key, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpsertAggregateAsync(string eTag, CustomerDataModel aggregate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IETagDto<CategoryIndex<LookupDataModel>>?> GetCategoryIndex(string categoryKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpsertCategoryIndex(string categoryKey, string eTag, CategoryIndex<LookupDataModel> categoryIndex,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
