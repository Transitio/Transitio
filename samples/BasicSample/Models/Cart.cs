public class Cart
{
    public string Id { get; set; } = string.Empty;    
    public List<Order> Orders { get; set; } = new();
}

public class CartDto
{
    public string Id { get; set; } = string.Empty;    
    public List<OrderDto> Orders { get; set; } = new();
}