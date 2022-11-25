using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jcg.CategorizedRepository.Api;

namespace Example.Types
{
    public class CustomerDataModel : IAggregateDataModel
    {
        public string Key { get; set; } = "";

        public Guid Id { get; set; }
        public string Name { get; set; } = "";

        public IEnumerable<OrderDataModel> Orders { get; set; }
        = Array.Empty<OrderDataModel>();
    }
}
