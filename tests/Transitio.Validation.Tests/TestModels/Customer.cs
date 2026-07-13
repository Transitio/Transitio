using System.Collections.Generic;

namespace Transitio.Validation.Tests;

public class Customer
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
    public List<string> Tags { get; set; } = new();
}
