using Example.Domain;
using Example.Types;
using Jcg.CategorizedRepository.Api;

namespace Example.Parts
{
    public class AggregateMapper
        : IAggregateMapper<Customer, CustomerDataModel>
    {
        /// <inheritdoc />
        public Customer ToAggregate(CustomerDataModel databaseModel)
        {
            var result = new Customer(databaseModel.Id, databaseModel.Name);

            foreach (var order in databaseModel.Orders)
            {
                result.AddOrder(order.Id);
            }

            return result;
        }

        /// <inheritdoc />
        public CustomerDataModel ToDatabaseModel(Customer aggregate)
        {
            return new()
            {
                Id = aggregate.Id,
                Name = aggregate.Name,
                Orders = aggregate.Orders.Select(Map).ToArray()
            };
        }

        private OrderDataModel Map(Order input)
        {
            return new()
            {
                Id = input.Id
            };
        }
    }
}