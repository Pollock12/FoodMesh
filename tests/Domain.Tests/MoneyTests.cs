using FluentAssertions;
using FoodMesh.Domain.ValueObjects;
using FoodMesh.Shared.Common;
using Xunit;

namespace FoodMesh.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Money_Should_Throw_When_Amount_Is_Negative()
    {
        // Act
        Action act = () => new Money(-5m, "USD");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Money_Should_Add_Amounts_With_Same_Currency()
    {
        // Arrange
        var m1 = new Money(15.50m, "USD");
        var m2 = new Money(10.25m, "USD");

        // Act
        var result = m1 + m2;

        // Assert
        result.Amount.Should().Be(25.75m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Money_Should_Throw_When_Adding_Different_Currencies()
    {
        // Arrange
        var usd = new Money(10m, "USD");
        var eur = new Money(10m, "EUR");

        // Act
        Action act = () => _ = usd + eur;

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot operate on different currencies*");
    }

    [Fact]
    public void Money_Should_Multiply_By_Quantity()
    {
        // Arrange
        var price = new Money(12.50m, "USD");

        // Act
        var total = price * 3;

        // Assert
        total.Amount.Should().Be(37.50m);
    }
}
