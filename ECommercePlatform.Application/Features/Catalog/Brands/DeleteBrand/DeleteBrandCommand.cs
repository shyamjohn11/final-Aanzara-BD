using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Brands.DeleteBrand;

public sealed record DeleteBrandCommand(Guid BrandId) : ICommand<Result>;