namespace FoodMesh.Shared.Common;

/// <summary>
/// Exception thrown when a domain business rule or invariant is violated.
/// For example: attempting to deliver an order that is not yet prepared or paid.
/// </summary>
public class DomainException : Exception
{
    public string? ErrorCode { get; }

    public DomainException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, Exception innerException, string? errorCode = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
