#nullable enable
using System;
using Xunit;

namespace Transitio.Mapper.Tests;

public class IncludeTests
{
    // Base classes
    public class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class Employee : Person
    {
        public string Department { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }

    public class Manager : Employee
    {
        public int TeamSize { get; set; }
    }

    // DTOs
    public class PersonDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class EmployeeDto : PersonDto
    {
        public string Department { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }

    public class ManagerDto : EmployeeDto
    {
        public int TeamSize { get; set; }
    }

    // Additional test classes
    public class Team
    {
        public string Name { get; set; } = string.Empty;
        public Employee[] Members { get; set; } = Array.Empty<Employee>();
    }

    public class TeamDto
    {
        public string Name { get; set; } = string.Empty;
        public EmployeeDto[] Members { get; set; } = Array.Empty<EmployeeDto>();
    }

    [Fact]
    public void Should_Include_Base_Type_Mapping()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>();
            cfg.CreateMap<Employee, EmployeeDto>()
                .Include<Person, PersonDto>();
        });

        var mapper = config.BuildMapper();

        var employee = new Employee
        {
            Name = "John",
            Age = 30,
            Department = "Engineering",
            Salary = 5000m
        };

        // Act
        var employeeDto = mapper.Map<EmployeeDto>(employee);

        // Assert
        Assert.NotNull(employeeDto);
        Assert.Equal("John", employeeDto.Name);
        Assert.Equal(30, employeeDto.Age);
        Assert.Equal("Engineering", employeeDto.Department);
        Assert.Equal(5000m, employeeDto.Salary);
    }

    [Fact]
    public void Should_Override_Base_Mapping_With_Derived_ForMember()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>();
            cfg.CreateMap<Employee, EmployeeDto>()
                .Include<Person, PersonDto>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.ToUpper()));
        });

        var mapper = config.BuildMapper();

        var employee = new Employee
        {
            Name = "john",
            Age = 30,
            Department = "Sales",
            Salary = 4000m
        };

        // Act
        var employeeDto = mapper.Map<EmployeeDto>(employee);

        // Assert
        Assert.NotNull(employeeDto);
        Assert.Equal("JOHN", employeeDto.Name);  // Overridden by derived mapping
        Assert.Equal(30, employeeDto.Age);
        Assert.Equal("Sales", employeeDto.Department);
    }

    [Fact]
    public void Should_Support_Multiple_Level_Include()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>();
            cfg.CreateMap<Employee, EmployeeDto>()
                .Include<Person, PersonDto>();
            cfg.CreateMap<Manager, ManagerDto>()
                .Include<Employee, EmployeeDto>();
        });

        var mapper = config.BuildMapper();

        var manager = new Manager
        {
            Name = "Alice",
            Age = 45,
            Department = "Engineering",
            Salary = 8000m,
            TeamSize = 10
        };

        // Act
        var managerDto = mapper.Map<ManagerDto>(manager);

        // Assert
        Assert.NotNull(managerDto);
        Assert.Equal("Alice", managerDto.Name);
        Assert.Equal(45, managerDto.Age);
        Assert.Equal("Engineering", managerDto.Department);
        Assert.Equal(8000m, managerDto.Salary);
        Assert.Equal(10, managerDto.TeamSize);
    }

    [Fact]
    public void Should_Use_IncludeBase_Alias()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>();
            cfg.CreateMap<Employee, EmployeeDto>()
                .IncludeBase<Person, PersonDto>();
        });

        var mapper = config.BuildMapper();

        var employee = new Employee
        {
            Name = "Bob",
            Age = 35,
            Department = "IT",
            Salary = 6000m
        };

        // Act
        var employeeDto = mapper.Map<EmployeeDto>(employee);

        // Assert
        Assert.NotNull(employeeDto);
        Assert.Equal("Bob", employeeDto.Name);
        Assert.Equal(35, employeeDto.Age);
    }

    [Fact]
    public void Should_Detect_Invalid_Include_Non_Base_Type()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            new TransitioMapperConfiguration(cfg =>
            {
                cfg.CreateMap<Person, PersonDto>();
                cfg.CreateMap<Employee, EmployeeDto>()
                    .Include<Manager, ManagerDto>();  // Invalid: Manager is not base of Employee
            });
        });

        Assert.Contains("is not a base type", ex.Message);
    }

    [Fact]
    public void Should_Apply_Derived_Condition_Over_Base()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>()
                .ForMember(d => d.Age, opt => opt.Condition(s => s.Age > 0));

            cfg.CreateMap<Employee, EmployeeDto>()
                .Include<Person, PersonDto>()
                .ForMember(d => d.Age, opt => opt.Condition(s => s.Age > 18));
        });

        var mapper = config.BuildMapper();

        var employee = new Employee
        {
            Name = "Young",
            Age = 10,
            Department = "Intern",
            Salary = 0m
        };

        // Act
        var employeeDto = mapper.Map<EmployeeDto>(employee);

        // Assert
        Assert.NotNull(employeeDto);
        Assert.Equal("Young", employeeDto.Name);
        Assert.Equal(0, employeeDto.Age);  // Condition from derived mapping (Age > 18) failed, so default value
    }

    [Fact]
    public void Should_Apply_Ignore_From_Derived()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>();
            cfg.CreateMap<Employee, EmployeeDto>()
                .Include<Person, PersonDto>()
                .ForMember(d => d.Salary, opt => opt.Ignore());
        });

        var mapper = config.BuildMapper();

        var employee = new Employee
        {
            Name = "Charlie",
            Age = 40,
            Department = "HR",
            Salary = 5500m
        };

        // Act
        var employeeDto = mapper.Map<EmployeeDto>(employee);

        // Assert
        Assert.NotNull(employeeDto);
        Assert.Equal("Charlie", employeeDto.Name);
        Assert.Equal(0m, employeeDto.Salary);  // Ignored
    }

    [Fact]
    public void Should_Chain_Includes_With_Other_Configurations()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>();
            cfg.CreateMap<Employee, EmployeeDto>()
                .Include<Person, PersonDto>()
                .ForMember(d => d.Department, opt => opt.MapFrom(s => s.Department.ToUpper()))
                .ForMember(d => d.Salary, opt => opt.MapFrom(s => s.Salary * 1.1m));
        });

        var mapper = config.BuildMapper();

        var employee = new Employee
        {
            Name = "Diana",
            Age = 28,
            Department = "marketing",
            Salary = 4000m
        };

        // Act
        var employeeDto = mapper.Map<EmployeeDto>(employee);

        // Assert
        Assert.NotNull(employeeDto);
        Assert.Equal("Diana", employeeDto.Name);
        Assert.Equal("MARKETING", employeeDto.Department);
        Assert.Equal(4400m, employeeDto.Salary);  // 4000 * 1.1
    }

    [Fact]
    public void Should_Include_With_Converter()
    {
        // Arrange - when ConvertUsing is used, it completely replaces Include behavior
        // The converter must handle all mapping logic
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>();
            cfg.CreateMap<Employee, EmployeeDto>()
                .Include<Person, PersonDto>()
                .ConvertUsing((source, context) =>
                {
                    // When converter is used, it takes full control
                    // We can use the mapper to map nested types if needed
                    var dto = new EmployeeDto
                    {
                        Name = source.Name.ToUpper(),
                        Age = source.Age,
                        Department = source.Department.ToUpper(),
                        Salary = source.Salary
                    };
                    return dto;
                });
        });

        var mapper = config.BuildMapper();

        var employee = new Employee
        {
            Name = "eve",
            Age = 32,
            Department = "operations",
            Salary = 5500m
        };

        // Act
        var employeeDto = mapper.Map<EmployeeDto>(employee);

        // Assert
        Assert.NotNull(employeeDto);
        Assert.Equal("EVE", employeeDto.Name);
        Assert.Equal("OPERATIONS", employeeDto.Department);
    }

    [Fact]
    public void Should_Support_Include_With_Recursive_Mapping()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>();
            cfg.CreateMap<Employee, EmployeeDto>()
                .Include<Person, PersonDto>();
            cfg.CreateMap<Team, TeamDto>()
                .ForMember(d => d.Members, opt => opt.MapFrom(
                    (s, ctx) => s.Members.Select(m => ctx.Mapper!.Map<EmployeeDto>(m)).ToArray()
                ));
        });

        var mapper = config.BuildMapper();

        var team = new Team
        {
            Name = "Dev Team",
            Members = new[]
            {
                new Employee { Name = "Frank", Age = 30, Department = "Dev", Salary = 5000m },
                new Employee { Name = "Grace", Age = 28, Department = "QA", Salary = 4500m }
            }
        };

        // Act
        var teamDto = mapper.Map<TeamDto>(team);

        // Assert
        Assert.NotNull(teamDto);
        Assert.Equal("Dev Team", teamDto.Name);
        Assert.Equal(2, teamDto.Members.Length);
        Assert.Equal("Frank", teamDto.Members[0].Name);
        Assert.Equal(30, teamDto.Members[0].Age);
        Assert.Equal("Grace", teamDto.Members[1].Name);
    }

    [Fact]
    public void Should_Preserve_Base_Ignore_In_Derived()
    {
        // Arrange
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>()
                .ForMember(d => d.Age, opt => opt.Ignore());

            cfg.CreateMap<Employee, EmployeeDto>()
                .Include<Person, PersonDto>();
        });

        var mapper = config.BuildMapper();

        var employee = new Employee
        {
            Name = "Henry",
            Age = 50,
            Department = "Management",
            Salary = 7000m
        };

        // Act
        var employeeDto = mapper.Map<EmployeeDto>(employee);

        // Assert
        Assert.NotNull(employeeDto);
        Assert.Equal("Henry", employeeDto.Name);
        Assert.Equal(0, employeeDto.Age);  // Ignored from base mapping
        Assert.Equal("Management", employeeDto.Department);
    }

    [Fact]
    public void Should_Auto_Map_Team_Members_Collection_Without_ForMember()
    {
        // Arrange - no ForMember for Members; relies on automatic nested-collection mapping 
        // of Employee[] -> EmployeeDto[], with Employee mapping reusing the Person base via include.
        var config = new TransitioMapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>();
            cfg.CreateMap<Employee, EmployeeDto>()
            .Include<Person, PersonDto>();
            cfg.CreateMap<Team, TeamDto>();
        });

        var mapper = config.BuildMapper();
        var team = new Team
        {
            Name = "Dev Team",
            Members = new[]
            {
                new Employee{Name = "Frank", Age=30,Department="Dev",Salary=5000m},
                new Employee{Name = "Grace", Age=28,Department="QA",Salary=4500m},
            }
        };

        //Act
        var teamDto = mapper.Map<TeamDto>(team);

        //Assert
        Assert.NotNull(teamDto);
        Assert.Equal("Dev Team", teamDto.Name);
        Assert.Equal(2, teamDto.Members.Length);
        Assert.Equal("Frank", teamDto.Members[0].Name);
        Assert.Equal(30, teamDto.Members[0].Age);
        Assert.Equal("Dev", teamDto.Members[0].Department);
        Assert.Equal(5000m, teamDto.Members[0].Salary);
        Assert.Equal("Grace", teamDto.Members[0].Name);
        Assert.Equal(4500m, teamDto.Members[0].Salary);
    }
}
