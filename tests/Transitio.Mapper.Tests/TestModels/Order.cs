namespace Transitio.Mapper.Tests;

public class Order
{
    public string Id { get; set; } = "";
    public User Customer { get; set; } = new();
}
