using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.ApplyCoupon;

public sealed record ApplyCouponCommand(Guid UserId, string Code)
    : ICommand<Result<CartSummaryResponse>>;

public sealed record RemoveCouponCommand(Guid UserId)
    : ICommand<Result<CartSummaryResponse>>;
