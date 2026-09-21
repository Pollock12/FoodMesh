using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Entities;
using FoodMesh.Domain.Models;
using FoodMesh.Domain.Services;

namespace FoodMesh.Domain.DomainServices;

/// <summary>
/// Domain service implementing cross-aggregate coordination between Order and DeliveryPartner.
/// Replaces distributed Saga complexity with cohesive in-domain orchestration.
/// </summary>
public sealed class OrderFulfillmentDomainService : IOrderFulfillmentDomainService
{
    public FulfillmentResult AssignBestAvailableRider(
        Order order,
        IEnumerable<DeliveryPartner> availablePartners)
    {
        if (order is null)
            return FulfillmentResult.Failed("Order cannot be null.");

        var candidates = availablePartners
            .Where(p => p.IsAvailable && !p.ActiveOrderId.HasValue)
            .ToList();

        if (candidates.Count == 0)
            return FulfillmentResult.Failed("No available delivery partners at this time.");

        // Select the first available delivery partner
        var bestPartner = candidates.First();

        // Cross-aggregate state update
        bestPartner.AssignOrder(order.Id);
        order.AssignDeliveryPartner(bestPartner.Id);

        return FulfillmentResult.Succeeded(bestPartner.Id);
    }

    public void CancelFulfillment(Order order, DeliveryPartner? partner, string reason)
    {
        if (order is null) return;

        order.Cancel(reason);

        // If a partner was previously assigned, release them back to the available pool
        if (partner != null && partner.ActiveOrderId == order.Id)
        {
            partner.CompleteOrder();
        }
    }
}
