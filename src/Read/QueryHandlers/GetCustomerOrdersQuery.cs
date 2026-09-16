using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using FoodMesh.Shared.SharedDto;
using MediatR;

namespace FoodMesh.Read.QueryHandlers;

/// <summary>
/// Query to fetch paginated order history for a customer.
/// </summary>
public sealed record GetCustomerOrdersQuery(
    Guid CustomerId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<OrderSummaryDto>>>;
