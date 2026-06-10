#nullable enable
using System.Collections.Generic;
using System.Linq;
using Transitio.Mapper;
using Xunit;

namespace Transitio.Mapper.Tests;

/// <summary>
/// Verifies that mapping a collection whose element map is a full converter (ConvertUsing)
/// applies the converter to every element.
/// </summary>
public class CollectionConverterTests
{
    public class Account
    {
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
    }
    public class AccountDto
    {
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
    }
    public class AccountConverter : ITypeConverter<Account, AccountDto>
    {
        public AccountDto Convert(Account source, IMappingContext context)
        => new AccountDto { Name = source.Name.ToUpper(), Score = source.Score * 2 };
    }

    [Fact]
    public void Should_Apply_Converter_To_Each_Element_In_Collection()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Account, AccountDto>().ConvertUsing<AccountConverter>();
        });

        var mapper = config.BuildMapper();
        var accounts = new List<Account>
        {
            new(){ Name = "john", Score = 10 },
            new(){ Name = "amy", Score = 20 }
        };

        var dtos = mapper.Map<List<AccountDto>>(accounts);
        Assert.Equal(2, dtos.Count);
        Assert.Equal("JOHN", dtos[0].Name);
        Assert.Equal(20, dtos[0].Score);
        Assert.Equal("AMY", dtos[1].Name);
        Assert.Equal(40, dtos[1].Score);
    }

    [Fact]
    public void Should_Apply_Converter_To_Array_Destination()
    {
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Account, AccountDto>().ConvertUsing<AccountConverter>();
        });

        var mapper = config.BuildMapper();

        var dtos = mapper.Map<AccountDto[]>(new[] { new Account { Name = "z", Score = 5 } });
        Assert.Single(dtos);
        Assert.Equal("Z", dtos[0].Name);
        Assert.Equal(10, dtos[0].Score);
    }
}