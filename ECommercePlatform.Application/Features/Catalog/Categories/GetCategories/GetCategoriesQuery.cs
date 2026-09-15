using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.GetCategories;

public sealed record GetCategoriesQuery : IQuery<Result<PagedResult<CategoryResponse>>>
{
    [MaxLength(200)]
    public string? Search { get; init; }

    public bool? IsActive { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    // Capped so a caller cannot ask for the whole table in one request.
    [Range(1, 200)]
    public int PageSize { get; init; } = 25;
}
