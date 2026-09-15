using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetProductById;

public sealed class GetProductByIdQueryHandler
    : IQueryHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    private readonly IProductRepository _products;

    public GetProductByIdQueryHandler(IProductRepository products) => _products = products;

    public async Task<Result<ProductResponse>> Handle(
        GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(request.ProductId, cancellationToken);

        return product is null
            ? Result.Failure<ProductResponse>(CatalogErrors.ProductNotFound)
            : Result.Success(product.ToResponse());
    }
}
