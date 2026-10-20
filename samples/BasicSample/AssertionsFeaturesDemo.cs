using Transitio.Assertions;

namespace BasicSample.AssertionsFeatures;

// ============================================================================
// Support types used only by the Transitio.Assertions feature demo.
// ============================================================================

public interface IShape
{
}

public sealed class Circle : IShape
{
}

public sealed class Square : IShape
{
}

// Mirrors the docs' own narrative examples (Withdraw/Ping/SaveAsync) so the demo reads the same
// way the documentation does.
public sealed class Account
{
    public void Withdraw(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must not be negative.", nameof(amount));
    }

    public void Ping()
    {
        // No-op: used to demonstrate NotThrow().
    }

    public Task SaveAsync(object? entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        return Task.CompletedTask;
    }

    public void Reconcile()
        => throw new InvalidOperationException("Reconcile failed.", new TimeoutException("Ledger service timed out."));
}

// A custom IComparable<T> struct - exercises the generic Should<T>() fallback added to
// Transitio.Assertions alongside this demo, since docs/assertions.md documents ordering/range
// assertions for "any other struct that is IComparable<T>", not just the built-in DateTime/
// DateTimeOffset/TimeSpan/char types.
public readonly struct Money : IComparable<Money>
{
    public Money(decimal amount) => Amount = amount;
    public decimal Amount { get; }
    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);
}

public record Point(int X, int Y);

public static class AssertionsFeaturesDemo
{
    public static void Run()
    {
        Console.WriteLine("=== Transitio.Assertions features ===");

        // 1. Entry point, chaining, and the "because" reason.
        5.Should().Be(5).And.NotBe(6);
        try
        {
            var total = -1;
            total.Should().BeGreaterThan(0, "an order always has a positive total");
        }
        catch (AssertionException ex)
        {
            Console.WriteLine($"Failure message includes the 'because' reason: {ex.Message}");
        }

        // 2. Objects: equality, nullability, type.
        var pointA = new Point(1, 2);
        var pointB = new Point(1, 2); // equal by value (record), different instance
        pointA.Should().NotBeNull();
        pointA.Should().Be(pointB).And.NotBe(new Point(9, 9));
        IShape shape = new Circle();
        shape.Should().BeOfType<Circle>();
        shape.Should().BeAssignableTo<IShape>();
        pointA.Should().BeSameAs(pointA);
        Console.WriteLine("Objects: NotNull/Be/NotBe/BeOfType/BeAssignableTo/BeSameAs all passed.");

        // 3. Booleans.
        true.Should().BeTrue();
        false.Should().BeFalse("the payload was valid");
        Console.WriteLine("Booleans: BeTrue/BeFalse passed.");

        // 4. Strings.
        "order-123".Should().Be("order-123").And.HaveLength(9);
        "Order saved".Should().Contain("saved").And.StartWith("Order").And.EndWith("saved");
        "Ada".Should().NotBeNullOrWhiteSpace();
        "order-42".Should().Match("order-*");
        Console.WriteLine("Strings: Be/HaveLength/Contain/StartWith/EndWith/NotBeNullOrWhiteSpace/Match passed.");

        // 5. Numbers.
        36.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(120);
        50m.Should().BeInRange(1m, 100m);
        3.Should().BePositive();
        (-3).Should().BeNegative();
        (0.1 + 0.2).Should().BeApproximately(0.3, 1e-9); // never compare floats with Be
        Console.WriteLine("Numbers: ordering/range/sign/BeApproximately passed.");

        // 6. Dates, durations, and other comparables - including a custom IComparable<T> struct.
        new DateTime(2026, 7, 15).Should().BeGreaterThan(new DateTime(2026, 1, 1));
        TimeSpan.FromMinutes(30).Should().BeInRange(TimeSpan.Zero, TimeSpan.FromHours(1));
        'B'.Should().BeGreaterThanOrEqualTo('B');
        new Money(10m).Should().BeGreaterThan(new Money(5m)).And.BeInRange(new Money(0m), new Money(20m));
        Console.WriteLine("Comparables: DateTime/TimeSpan/char/custom Money struct passed.");

        // 7. Collections.
        var orders = new[] { "Ada", "Alan", "Grace" };
        orders.Should().NotBeEmpty().And.HaveCount(3);
        orders.Should().Contain("Ada").And.Contain(n => n.StartsWith("Al"));
        new[] { 2, 4, 6 }.Should().OnlyContain(a => a % 2 == 0);
        new[] { 42 }.Should().ContainSingle();
        new[] { 1, 2, 3 }.Should().BeEquivalentTo(new[] { 3, 2, 1 });
        Console.WriteLine("Collections: NotBeEmpty/HaveCount/Contain/OnlyContain/ContainSingle/BeEquivalentTo passed.");

        // 8. Dictionaries - plus the IDictionary-vs-IReadOnlyDictionary resolution gotcha.
        var scores = new Dictionary<string, int> { ["alice"] = 42, ["bob"] = 7 };
        scores.Should().ContainKey("alice").And.ContainValue(42);
        scores.Should().NotContainKey("legacy-flag");

        IDictionary<string, int> asDictionary = scores;
        IReadOnlyDictionary<string, int> asReadOnlyDictionary = scores;
        var dictionaryAssertion = asDictionary.Should();
        var collectionAssertion = asReadOnlyDictionary.Should(); // falls through: IReadOnlyDictionary doesn't implement IDictionary
        Console.WriteLine($"Dictionary resolution gotcha: IDictionary<,> -> {dictionaryAssertion.GetType().Name}, " +
            $"IReadOnlyDictionary<,> (same instance) -> {collectionAssertion.GetType().Name}");

        // 9. Guid - deliberately restricted: no ordering (Guid ordering isn't meaningful) and no
        //    BeSameAs (a boxed Guid's reference never reflects value equality).
        var id = Guid.NewGuid();
        id.Should().NotBe(Guid.Empty);
        id.Should().Be(id).And.NotBeNull();
        Console.WriteLine("Guid: Be/NotBe/BeNull/NotBeNull passed (no ordering/BeSameAs by design).");

        // 10. Exceptions, sync and async.
        var account = new Account();
        Action withdraw = () => account.Withdraw(-1);
        withdraw.Should().Throw<ArgumentException>()
            .WithMessage("*amount*")
            .Which.ParamName.Should().Be("amount");

        Action reconcile = account.Reconcile;
        reconcile.Should().Throw<InvalidOperationException>()
            .WithInnerException<TimeoutException>();

        Action ping = account.Ping;
        ping.Should().NotThrow();

        Console.WriteLine("Exceptions (sync): Throw/WithMessage/Which/WithInnerException/NotThrow passed.");

        RunAsyncExceptionAssertions(account).GetAwaiter().GetResult();
    }

    private static async Task RunAsyncExceptionAssertions(Account account)
    {
        Func<Task> saveNull = () => account.SaveAsync(null);
        await saveNull.Should().ThrowAsync<ArgumentNullException>();

        Func<Task> saveValid = () => account.SaveAsync(new object());
        await saveValid.Should().NotThrowAsync();

        Console.WriteLine("Exceptions (async): ThrowAsync/NotThrowAsync passed.");
    }
}