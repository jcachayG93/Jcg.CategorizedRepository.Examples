namespace Example.Types
{
    public class CustomerDataModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";

        public IEnumerable<OrderDataModel> Orders { get; set; }
            = Array.Empty<OrderDataModel>();
    }
}