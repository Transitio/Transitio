namespace Transitio.Mapper.Tests;

public class OrderDto
{
    public string Id { get; set; } = "";
    public UserDto Customer { get; set; } = new();
}