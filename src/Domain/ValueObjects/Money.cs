using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.ValueObjects;

/// <summary>
/// Immutable Value Object representing monetary amounts with currency.
/// Enforces business invariants such as currency matching and non-negative constraints.
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new DomainException("Monetary amount cannot be negative.", "INVALID_AMOUNT");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency must be specified.", "INVALID_CURRENCY");

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.Trim().ToUpperInvariant();
    }

    public static Money Zero(string currency = "USD") => new(0m, currency);

    public Money Add(Money other)
    {
        AssertSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        AssertSameCurrency(other);
        if (Amount < other.Amount)
            throw new DomainException("Cannot subtract a larger monetary amount from a smaller one.", "INSUFFICIENT_FUNDS");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("Quantity cannot be negative.", "INVALID_QUANTITY");

        return new Money(Amount * quantity, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new DomainException("Factor cannot be negative.", "INVALID_FACTOR");

        return new Money(Amount * factor, Currency);
    }

    private void AssertSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainException($"Cannot operate on different currencies: '{Currency}' and '{other.Currency}'.", "CURRENCY_MISMATCH");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        AssertSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, int quantity) => left.Multiply(quantity);
    public static Money operator *(Money left, decimal factor) => left.Multiply(factor);

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
