using Example.Types;
using Jcg.CategorizedRepository.Api;

namespace Example.Parts
{
    public class
        AggregateToLookupMapper : IAggregateToLookupMapper<CustomerDataModel,
            CustomerLookup>
    {
        /// <inheritdoc />
        public CustomerLookup ToLookup(CustomerDataModel aggregate)
        {
            return new()
            {
                CustomerId = aggregate.Id,
                Name = aggregate.Name,
                NumberOfOrders = aggregate.Orders.Count()
            };
        }
    }
}