// Intentionally invalid: 'Missing' has no matching source property on User. Used only to
// demonstrate cfg.ValidateConfiguration()'s eager, opt-in validation at construction time.
public class BrokenUserDto
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Missing { get; set; } = string.Empty;
}