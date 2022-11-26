using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Domain
{
    /// <summary>
    /// The Aggregate
    /// </summary>
    public class Customer
    {
        public Customer(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public void AddOrder(Guid id)
        {
            var order = new Order(id);
            _orders.Add(order);
        }

        public void UpdateName(string name)
        {
            Name = name;
        }

        public Guid Id { get; }

        public string Name { get; private set; }

        public IReadOnlyCollection<Order> Orders => _orders;

        private List<Order> _orders = new();
       
    }
}
