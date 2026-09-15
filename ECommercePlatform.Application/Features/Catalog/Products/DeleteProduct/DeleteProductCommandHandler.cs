using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.Products.DeleteProduct;

public sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IProductRepository products,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _products = products;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(CatalogErrors.ProductNotFound);
        }

        _products.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} deleted.", request.ProductId);

        return Result.Success();
    }
}
