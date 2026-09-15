using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.CreateCategory;

public sealed record CreateCategoryCommand : ICommand<Result<CategoryResponse>>
{
    [Required]
    [MaxLength(50)]
    public string CategoryCode { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CategoryName { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    public bool HasSubCategory { get; init; }

    public bool IsActive { get; init; } = true;

    /// <summary>Optional primary image, saved alongside the new category.</summary>
    public FileUpload? Image { get; init; }
}