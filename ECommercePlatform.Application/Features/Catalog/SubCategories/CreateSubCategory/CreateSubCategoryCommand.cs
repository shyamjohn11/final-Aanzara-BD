using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.SubCategories.CreateSubCategory;

public sealed record CreateSubCategoryCommand : ICommand<Result<SubCategoryResponse>>
{
    [Required]
    public Guid CategoryId { get; init; }

    [Required]
    [MaxLength(50)]
    public string SubCategoryCode { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SubCategoryName { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;
}
