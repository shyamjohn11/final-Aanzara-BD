using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.GetCategoriesWithSubCategories;

/// <summary>Lightweight id/name list of categories flagged as having sub-categories.</summary>
public sealed record GetCategoriesWithSubCategoriesQuery
    : IQuery<Result<IReadOnlyList<CategoryLookupResponse>>>;