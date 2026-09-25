using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Application.Features.Admin.Reviews;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Shop.Reviews;

/// <summary>
/// Storefront review submit. Unlike the admin manual create, this requires
/// the author to have bought the product: the caller must be authenticated
/// (UserId comes from the token, never the body) and hold a non-cancelled
/// order containing it. Accepted reviews are stored Pending (moderation)
/// and flagged as verified purchases, which drives the storefront
/// "Verified Buyer" badge.
/// </summary>
public sealed record SubmitReviewCommand : ICommand<Result<ReviewResponse>>
{
    public Guid UserId { get; init; }

    [MaxLength(300)] public string? ProductName { get; init; }

    [Range(1, 5)] public int Rating { get; init; } = 5;

    [MaxLength(4000)] public string? Comment { get; init; }
}

public sealed class SubmitReviewCommandHandler
    : ICommandHandler<SubmitReviewCommand, Result<ReviewResponse>>
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;
    private readonly IAdminRepository<Review> _reviews;
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitReviewCommandHandler(
        IOrderRepository orders,
        IUserRepository users,
        IAdminRepository<Review> reviews,
        IAdminRepository<Notification> notifications,
        IUnitOfWork unitOfWork)
    {
        _orders = orders;
        _users = users;
        _reviews = reviews;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewResponse>> Handle(
        SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        var productName = (request.ProductName ?? string.Empty).Trim();
        var comment = (request.Comment ?? string.Empty).Trim();

        if (productName.Length == 0)
        {
            return Result.Failure<ReviewResponse>(Error.Validation(
                "review.product_required", "Please tell us which product you are reviewing."));
        }

        if (request.Rating is < 1 or > 5)
        {
            return Result.Failure<ReviewResponse>(Error.Validation(
                "review.rating_invalid", "Rating must be between 1 and 5 stars."));
        }

        if (comment.Length == 0)
        {
            return Result.Failure<ReviewResponse>(Error.Validation(
                "review.comment_required", "Please write a few words about the product."));
        }

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<ReviewResponse>(Error.Unauthorized(
                "review.unauthenticated", "Please sign in to write a review."));
        }

        // Buyer check: a non-cancelled, non-returned order containing the
        // product (name-matched, case-insensitive — reviews are name-keyed
        // end to end on the storefront).
        var (orders, _) = await _orders.GetByUserIdAsync(request.UserId, 1, 100, cancellationToken);

        var purchased = orders
            .Where(o => o.OrderStatus != OrderStatus.Cancelled
                && o.OrderStatus != OrderStatus.Returned)
            .SelectMany(o => o.OrderItems)
            .Select(i => i.Product?.ProductName)
            .Any(n => !string.IsNullOrWhiteSpace(n)
                && string.Equals(n.Trim(), productName, StringComparison.OrdinalIgnoreCase));

        if (!purchased)
        {
            return Result.Failure<ReviewResponse>(Error.Forbidden(
                "review.not_a_buyer", "Only customers who purchased this product can review it."));
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            ProductName = productName,
            CustomerName = user.Name,
            UserId = user.UserId,
            Rating = request.Rating,
            Comment = comment,
            IsVerifiedPurchase = true,
            // Client state is never trusted — storefront submits stay Pending.
            Status = "Pending"
        };

        _reviews.Add(review);
        NotificationEmitter.Emit(
            _notifications,
            "review",
            $"New verified review: {review.ProductName} ({review.Rating}/5)",
            $"From {review.CustomerName}.",
            "/admin/reviews");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(review.ToDto());
    }
}
