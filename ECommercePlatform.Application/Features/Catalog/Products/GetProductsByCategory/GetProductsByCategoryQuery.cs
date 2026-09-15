using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;
using ECommercePlatform.Application.Common.Abstractions;
namespace ECommercePlatform.Application.Features.Catalog.Products.GetProductsByCategory;

public sealed record GetProductsByCategoryQuery(
    Guid CategoryId,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<PagedResult<ProductSummaryResponse>>>;