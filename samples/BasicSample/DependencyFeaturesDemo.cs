using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Transitio.Dependency;
using Transitio.Mapper;

namespace BasicSample.DependencyFeatures;

// ============================================================================
// Models / services used only by the Transitio.Dependency feature demos.
// ============================================================================

public class Account
{
    public string Owner { get; set; } = "";
    public decimal Balance { get; set; }
}

public class AccountDto
{
    public string Owner { get; set; } = "";
    public decimal Balance { get; set; }
}

// Intentionally invalid: 'Missing' has no matching source property on Account.
// Used by the fail-fast validation demo.
public class BrokenAccountDto
{
    public string Owner { get; set; } = "";
    public decimal Balance { get; set; }
    public string Missing { get; set; } = "";
}

// A profile discovered automatically by assembly scanning.
public class AccountProfile : MappingProfile
{
    public override void Configure(TransitioConfigBuilder cfg)
        => cfg.CreateMap<Account, AccountDto>();
}

// A scoped service consumed by a converter – demonstrates that a Scoped mapper
// resolves its converter (and the converter's dependencies) from the active scope.
public class RequestContext
{
    public string CorrelationId { get; set; } = "";
}

public class StampInput
{
    public string Message { get; set; } = "";
}

public class StampedDto
{
    public string Message { get; set; } = "";
    public string CorrelationId { get; set; } = "";
}

public class StampingConverter : ITypeConverter<StampInput, StampedDto>
{
    private readonly RequestContext _context;
    public StampingConverter(RequestContext context) => _context = context;

    public StampedDto Convert(StampInput source, IMappingContext context)
        => new() { Message = source.Message, CorrelationId = _context.CorrelationId };
}

// A consumer that takes a direct dependency on IMapper.
public class AccountReportService
{
    private readonly IMapper _mapper;
    public AccountReportService(IMapper mapper) => _mapper = mapper;

    public AccountDto Report(Account account) => _mapper.Map<AccountDto>(account);
}

// ============================================================================
// Demos for the Transitio.Dependency (DI) features.
// ============================================================================

public static class DependencyFeaturesDemo
{
    public static void Run()
    {
        Console.WriteLine("################################################");
        Console.WriteLine("##  Transitio.Dependency feature demos      ##");
        Console.WriteLine("################################################");
        Console.WriteLine();

        RunAssemblyScanningDemo();
        RunDirectInjectionDemo();
        RunKeyedMapperDemo();
        RunScopedConverterDemo();
        RunValidationDemo();
    }

    // 1) Assembly scanning: discover MappingProfile types automatically.
    private static void RunAssemblyScanningDemo()
    {
        Console.WriteLine("=== Assembly Scanning (auto-discover profiles) ===");

        var services = new ServiceCollection();
        services.AddTransitio(typeof(AccountProfile).Assembly); // scans this assembly

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var dto = mapper.Map<AccountDto>(new Account { Owner = "Hitesh", Balance = 1500m });
        Console.WriteLine($"- AccountProfile was discovered: {dto.Owner} / {dto.Balance:C}");
        Console.WriteLine();
    }

    // 2) Direct IMapper injection into a consumer service.
    private static void RunDirectInjectionDemo()
    {
        Console.WriteLine("=== Direct IMapper Injection ===");

        var services = new ServiceCollection();
        services.AddTransitio(cfg => cfg.CreateMap<Account, AccountDto>());
        services.AddSingleton<AccountReportService>();

        var provider = services.BuildServiceProvider();
        var report = provider.GetRequiredService<AccountReportService>();

        var dto = report.Report(new Account { Owner = "Ada", Balance = 999m });
        Console.WriteLine($"- AccountReportService(IMapper) produced: {dto.Owner} / {dto.Balance:C}");
        Console.WriteLine();
    }

    // 3) Keyed mappers: multiple independent configurations side by side.
    private static void RunKeyedMapperDemo()
    {
        Console.WriteLine("=== Keyed Mappers ===");

        var services = new ServiceCollection();

        // "masked" hides the owner; "plain" copies it through.
        services.AddKeyedTransitio("masked", cfg =>
            cfg.CreateMap<Account, AccountDto>()
                .ForMember(d => d.Owner, o => o.MapFrom(s => Mask(s.Owner))));

        services.AddKeyedTransitio("plain", cfg => cfg.CreateMap<Account, AccountDto>());

        var provider = services.BuildServiceProvider();

        var masked = provider.GetRequiredKeyedService<IMapper>("masked");
        var plain = provider.GetRequiredKeyedService<IMapper>("plain");

        var account = new Account { Owner = "Hitesh", Balance = 50m };
        Console.WriteLine($"- masked: {masked.Map<AccountDto>(account).Owner}");
        Console.WriteLine($"- plain:  {plain.Map<AccountDto>(account).Owner}");
        Console.WriteLine();
    }

    // 4) Scoped lifetime: a converter resolves a scoped dependency per scope.
    private static void RunScopedConverterDemo()
    {
        Console.WriteLine("=== Scoped Converter (per-scope dependency) ===");

        var services = new ServiceCollection();
        services.AddScoped<RequestContext>();
        services.AddTransitio(
            ServiceLifetime.Scoped,
            cfg => cfg.CreateMap<StampInput, StampedDto>().ConvertUsing<StampingConverter>());

        // validateScopes proves the mapper is genuinely scope-bound (no captive dependency).
        var provider = services.BuildServiceProvider(validateScopes: true);

        foreach (var id in new[] { "REQ-1", "REQ-2" })
        {
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<RequestContext>().CorrelationId = id;

            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var dto = mapper.Map<StampedDto>(new StampInput { Message = "hello" });
            Console.WriteLine($"- scope {id}: stamped with '{dto.CorrelationId}'");
        }
        Console.WriteLine();
    }

    // 5) Fail-fast validation: an invalid map throws at registration time.
    private static void RunValidationDemo()
    {
        Console.WriteLine("=== Fail-Fast Validation ===");

        var services = new ServiceCollection();
        try
        {
            services.AddTransitio(cfg =>
            {
                cfg.ValidateConfiguration();                  // opt in
                cfg.CreateMap<Account, BrokenAccountDto>(); // BrokenAccountDto.Missing has no source
            });

            Console.WriteLine("- (unexpected) registration succeeded");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"- caught at startup as expected: {ex.Message.Split('\n')[0]}");
        }
        Console.WriteLine();
    }

    private static string Mask(string value)
        => value.Length <= 1 ? "*" : value[0] + new string('*', value.Length - 1);
}