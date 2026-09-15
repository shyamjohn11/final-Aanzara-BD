using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.UpdateProduct;

public sealed record UpdateProductCommand : ProductWriteModel, ICommand<Result<ProductResponse>>
{
    /// <summary>Comes from the route, not the body.</summary>
    [JsonIgnore]
    public Guid ProductId { get; init; }
}
