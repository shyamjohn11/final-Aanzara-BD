using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Categories.GetProductTree;

/// <summary>
/// The whole product tree in one call: Category -> SubCategory, with product
/// counts at each node.
/// </summary>
/// <param name="ActiveOnly">
/// When true, inactive categories and sub-categories are omitted — what a
/// storefront wants. Admin screens pass false to see everything.
/// </param>
public sealed record GetProductTreeQuery(bool ActiveOnly = true) : IQuery<Result<ProductTreeResponse>>;
