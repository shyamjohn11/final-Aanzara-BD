using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.SubCategories.GetSubCategories;

public sealed record GetSubCategoriesQuery : IQuery<Result<PagedResult<SubCategoryResponse>>>
{
    public Guid? CategoryId { get; init; }

    [MaxLength(200)]
    public string? Search { get; init; }

    public bool? IsActive { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int PageSize { get; init; } = 25;
}
