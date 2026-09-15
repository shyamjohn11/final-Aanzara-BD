using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<Result<ProductResponse>>;
