using Jcg.CategorizedRepository.Api;

namespace Example.Types;

public class Lookup : IRepositoryLookup
{

    public Guid CustomerId { get; set; }

    public string Name { get; set; } = "";

    public int NumberOfOrders { get; set; }
    public string Key { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedTimeStamp { get; set; } = "";
}