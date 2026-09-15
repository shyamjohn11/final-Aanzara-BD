using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Brands.SearchBrands;

public sealed record SearchBrandsQuery(
    [property: MaxLength(200)] string? Keyword,
    [property: Range(1, 50)] int Limit = 10)
    : IQuery<Result<IReadOnlyList<BrandResponse>>>;