using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.ClearCart;

public sealed record ClearCartCommand(Guid UserId) : ICommand<Result>;
