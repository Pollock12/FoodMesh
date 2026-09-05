using FluentAssertions;
using FoodMesh.Shared.Common;
using Xunit;

namespace FoodMesh.Domain.Tests;

public class SharedTests
{
    // Test Entity subclass for verification
    private class TestEntity : Entity<Guid>
    {
        public string Name { get; }

        public TestEntity(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

    // Test Value Object subclass for verification
    private class TestMoney : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public TestMoney(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void Two_Entities_With_Same_Id_Should_Be_Equal()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Pizza");
        var entity2 = new TestEntity(id, "Burger");

        // Act & Assert (Entities are equal if their IDs match, even if properties differ)
        entity1.Should().Be(entity2);
        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void Two_ValueObjects_With_Same_Values_Should_Be_Equal()
    {
        // Arrange
        var money1 = new TestMoney(100.50m, "USD");
        var money2 = new TestMoney(100.50m, "USD");

        // Act & Assert (Value objects are equal if their fields match)
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Result_Success_Should_Contain_Value()
    {
        // Act
        var result = Result<string>.Success("Order Placed");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Order Placed");
    }

    [Fact]
    public void Result_Failure_Should_Contain_ErrorMessage()
    {
        // Act
        var result = Result<string>.Failure("Payment failed");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Payment failed");
    }
}
