using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Reviews;

// IDs #127-130 + POST/PUT (A1) — list, details, create, update,
// PATCH /{id}/status (approve/hide), delete.
// Persisted in SQL Server (Reviews table). Wire shape unchanged.

public sealed record ReviewResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public int Rating { get; init; } = 5;
    public string? Comment { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetReviewsQuery : IQuery<Result<PagedResult<ReviewResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetReviewByIdQuery(Guid Id) : IQuery<Result<ReviewResponse>>;

public sealed record UpdateReviewStatusCommand(Guid Id, string Status)
    : ICommand<Result<ReviewResponse>>;

public sealed record DeleteReviewCommand(Guid Id) : ICommand<Result>;

public sealed record CreateReviewCommand : ICommand<Result<ReviewResponse>>
{
    [MaxLength(300)] public string? ProductName { get; init; }
    [MaxLength(200)] public string? CustomerName { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [Range(1, 5)] public int Rating { get; init; } = 5;
    [MaxLength(4000)] public string? Comment { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateReviewCommand : ICommand<Result<ReviewResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(300)] public string? ProductName { get; init; }
    [MaxLength(200)] public string? CustomerName { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [Range(1, 5)] public int? Rating { get; init; }
    [MaxLength(4000)] public string? Comment { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

internal static class ReviewMappings
{
    internal static ReviewResponse ToDto(this Review review) => new()
    {
        Id = review.Id,
        ProductName = review.ProductName,
        CustomerName = review.CustomerName,
        Rating = review.Rating,
        Comment = review.Comment,
        Status = review.Status,
        CreatedAt = review.CreatedAt,
        UpdatedAt = review.UpdatedAt
    };
}

public sealed class GetReviewsQueryHandler
    : IQueryHandler<GetReviewsQuery, Result<PagedResult<ReviewResponse>>>
{
    private readonly IAdminRepository<Review> _reviews;

    public GetReviewsQueryHandler(IAdminRepository<Review> reviews) => _reviews = reviews;

    public async Task<Result<PagedResult<ReviewResponse>>> Handle(
        GetReviewsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<Review, bool>> filter = AdminFilters.True<Review>();

        var status = request.Status?.Trim();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = filter.And(r => r.Status == status);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(r =>
                r.ProductName.Contains(s)
                || r.CustomerName.Contains(s)
                || (r.Comment != null && r.Comment.Contains(s)));
        }

        var total = await _reviews.CountAsync(filter, cancellationToken);
        var items = await _reviews.PageAsync(
            filter,
            q => q.OrderByDescending(r => r.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<ReviewResponse>(
            items.Select(r => r.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class GetReviewByIdQueryHandler
    : IQueryHandler<GetReviewByIdQuery, Result<ReviewResponse>>
{
    private readonly IAdminRepository<Review> _reviews;

    public GetReviewByIdQueryHandler(IAdminRepository<Review> reviews) => _reviews = reviews;

    public async Task<Result<ReviewResponse>> Handle(
        GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _reviews.GetByIdAsync(request.Id, cancellationToken);

        return review is null
            ? Result.Failure<ReviewResponse>(AdminErrors.NotFound("Review", request.Id))
            : Result.Success(review.ToDto());
    }
}

public sealed class UpdateReviewStatusCommandHandler
    : IRequestHandler<UpdateReviewStatusCommand, Result<ReviewResponse>>
{
    private readonly IAdminRepository<Review> _reviews;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReviewStatusCommandHandler(
        IAdminRepository<Review> reviews, IUnitOfWork unitOfWork)
    {
        _reviews = reviews;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewResponse>> Handle(
        UpdateReviewStatusCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviews.GetByIdAsync(request.Id, cancellationToken);

        if (review is null)
        {
            return Result.Failure<ReviewResponse>(AdminErrors.NotFound("Review", request.Id));
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Result.Failure<ReviewResponse>(
                Error.Validation("admin.status_required", "Status is required."));
        }

        review.Status = request.Status.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(review.ToDto());
    }
}

public sealed class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Result>
{
    private readonly IAdminRepository<Review> _reviews;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReviewCommandHandler(IAdminRepository<Review> reviews, IUnitOfWork unitOfWork)
    {
        _reviews = reviews;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviews.GetByIdAsync(request.Id, cancellationToken);

        if (review is null)
        {
            return Result.Failure(AdminErrors.NotFound("Review", request.Id));
        }

        _reviews.Remove(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class CreateReviewCommandHandler
    : IRequestHandler<CreateReviewCommand, Result<ReviewResponse>>
{
    private readonly IAdminRepository<Review> _reviews;
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReviewCommandHandler(
        IAdminRepository<Review> reviews,
        IAdminRepository<Notification> notifications,
        IUnitOfWork unitOfWork)
    {
        _reviews = reviews;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewResponse>> Handle(
        CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ProductName = request.ProductName ?? "Product",
            CustomerName = request.CustomerName ?? request.Name ?? "Customer",
            Rating = request.Rating,
            Comment = request.Comment,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status!
        };

        _reviews.Add(review);
        NotificationEmitter.Emit(
            _notifications,
            "review",
            $"New review: {review.ProductName} ({review.Rating}/5)",
            $"From {review.CustomerName}.",
            "/admin/reviews");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(review.ToDto());
    }
}

public sealed class UpdateReviewCommandHandler
    : IRequestHandler<UpdateReviewCommand, Result<ReviewResponse>>
{
    private readonly IAdminRepository<Review> _reviews;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReviewCommandHandler(IAdminRepository<Review> reviews, IUnitOfWork unitOfWork)
    {
        _reviews = reviews;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewResponse>> Handle(
        UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviews.GetByIdAsync(request.Id, cancellationToken);

        if (review is null)
        {
            return Result.Failure<ReviewResponse>(AdminErrors.NotFound("Review", request.Id));
        }

        review.ProductName = request.ProductName ?? review.ProductName;
        review.CustomerName = request.CustomerName ?? request.Name ?? review.CustomerName;
        review.Rating = request.Rating ?? review.Rating;
        review.Comment = request.Comment ?? review.Comment;
        review.Status = request.Status ?? review.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(review.ToDto());
    }
}
