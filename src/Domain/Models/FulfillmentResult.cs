namespace FoodMesh.Domain.Models;

/// <summary>
/// Domain model encapsulating the result of an order fulfillment / rider dispatch operation.
/// </summary>
public sealed class FulfillmentResult
{
    public bool IsSuccess { get; }
    public Guid? AssignedPartnerId { get; }
    public string? ErrorMessage { get; }

    private FulfillmentResult(bool isSuccess, Guid? assignedPartnerId, string? errorMessage)
    {
        IsSuccess = isSuccess;
        AssignedPartnerId = assignedPartnerId;
        ErrorMessage = errorMessage;
    }

    public static FulfillmentResult Succeeded(Guid assignedPartnerId) =>
        new(true, assignedPartnerId, null);

    public static FulfillmentResult Failed(string errorMessage) =>
        new(false, null, errorMessage);
}
