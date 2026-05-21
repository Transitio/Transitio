public class Order
{
    public string Id { get; set; } = string.Empty;
    public User Customer { get; set; } = new();
}