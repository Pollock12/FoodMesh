using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Entities;
using FoodMesh.Domain.Models;

namespace FoodMesh.Domain.Services;

/// <summary>
/// Domain service interface for coordinating cross-aggregate order fulfillment,
/// matching orders with available delivery partners, and managing release/cancellation invariants.
/// </summary>
public interface IOrderFulfillmentDomainService
{
    FulfillmentResult AssignBestAvailableRider(Order order, IEnumerable<DeliveryPartner> availablePartners);
    void CancelFulfillment(Order order, DeliveryPartner? partner, string reason);
}
