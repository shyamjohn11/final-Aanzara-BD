using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetLowStockProducts;

public sealed record GetLowStockProductsQuery(int Count = 8)
    : IQuery<Result<IReadOnlyList<ProductSummaryResponse>>>;
