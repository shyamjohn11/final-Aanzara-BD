using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId) : ICommand<Result>;
