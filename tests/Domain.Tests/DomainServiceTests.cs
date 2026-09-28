using FluentAssertions;
using FoodMesh.Domain.DomainServices;
using FoodMesh.Domain.ValueObjects;
using Xunit;

namespace FoodMesh.Domain.Tests;

public class DomainServiceTests
{
    private readonly DeliveryAddress _customerAddress = new(
        street: "456 Avenue",
        city: "Dhaka",
        postalCode: "1212",
        contactPhoneNumber: "+8801700000000");

    [Fact]
    public void DeliveryFeeCalculator_Should_Return_Standard_Fee()
    {
        // Arrange
        var calculator = new DeliveryFeeCalculator();

        // Act
        var fee = calculator.CalculateFee(_customerAddress);

        // Assert
        fee.Amount.Should().Be(2.50m);
        fee.Currency.Should().Be("USD");
    }
}
