using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid CategoryId) : IQuery<Result<CategoryDetailResponse>>;
