public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public UserDto Customer { get; set; } = new();
}