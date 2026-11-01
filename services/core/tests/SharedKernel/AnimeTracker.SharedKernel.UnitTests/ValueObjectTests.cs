using AnimeTracker.SharedKernel;
using Shouldly;

namespace AnimeTracker.SharedKernel.UnitTests;

public sealed class ValueObjectTests
{
    private sealed class Money(decimal amount, string currency) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return amount;
            yield return currency;
        }
    }

    [Fact]
    public void Value_objects_with_same_components_are_equal()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");
        a.Equals(b).ShouldBeTrue();
        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void Value_objects_with_different_components_are_not_equal()
    {
        var a = new Money(10m, "USD");
        var b = new Money(20m, "USD");
        a.Equals(b).ShouldBeFalse();
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_returns_false_for_null()
    {
        var a = new Money(10m, "USD");
        a.Equals((ValueObject?)null).ShouldBeFalse();
        a.Equals((object?)null).ShouldBeFalse();
    }

    [Fact]
    public void Equal_value_objects_have_same_hash_code()
    {
        new Money(5m, "EUR").GetHashCode()
            .ShouldBe(new Money(5m, "EUR").GetHashCode());
    }

    [Fact]
    public void Operator_equality_handles_null_left_side()
    {
        Money? left = null;
        var right = new Money(1m, "USD");
        (left == right).ShouldBeFalse();
        (left == null).ShouldBeTrue();
    }
}
