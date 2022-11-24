using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Types
{
    public class CustomerDatabaseModel : IAggregateData
    {
        public Guid Id { get; set; }
    }
}
