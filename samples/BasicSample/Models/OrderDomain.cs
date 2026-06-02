
public class OrderDomain
{
    public decimal FinalAmount;
    public required string Region;
}

public class OrderDomain_Type : OrderDomain {}
public class OrderDomain_Instance : OrderDomain {}
public class OrderDomain_Delegate : OrderDomain {}