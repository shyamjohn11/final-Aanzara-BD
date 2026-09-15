using ECommercePlatform.Application.Common.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand : ICommand<Result<CategoryResponse>>
{
    /// <summary>Comes from the route, not the body.</summary>
    [JsonIgnore]
    public Guid CategoryId { get; init; }

    [Required]
    [MaxLength(50)]
    public string CategoryCode { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CategoryName { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;

    /// <summary>Optional image to set as primary for the category.</summary>
    public FileUpload? Image { get; init; }
}
