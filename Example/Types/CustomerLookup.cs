namespace Example.Types;

public class CustomerLookup
{
    public Guid CustomerId { get; set; }

    public string Name { get; set; } = "";

    public int NumberOfOrders { get; set; }
}