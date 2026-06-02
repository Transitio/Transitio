using Transitio.Mapper;

public class OrderConverter
    : ITypeConverter<OrderInput, OrderDomain_Type>
{  

    public OrderDomain_Type Convert(OrderInput source, IMappingContext context)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        // Example use of context (optional)
        var region = context?.Items.TryGetValue("RegionOverride", out var value) == true
            ? value?.ToString()
            : (source.Country == "IN" ? "APAC" : "GLOBAL");

        var finalAmount = source.Currency == "INR"
            ? source.Amount * 0.012m
            : source.Amount;

        return new OrderDomain_Type
        {
            FinalAmount = finalAmount,
            Region = region ?? "Standard"
        };
    }
}