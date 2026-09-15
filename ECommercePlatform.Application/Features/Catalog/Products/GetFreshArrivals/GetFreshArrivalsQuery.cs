using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetFreshArrivals;

public sealed record GetFreshArrivalsQuery(int Count = 10)
    : IQuery<Result<IReadOnlyList<ProductSummaryResponse>>>;