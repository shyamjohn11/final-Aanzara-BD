using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Content;

// IDs #150, #157-158 — newsletter subscriptions (public subscribe,
// admin list/delete). In-memory store like the other admin CMS modules.

public sealed record NewsletterSubscriptionResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Source { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record SubscribeNewsletterCommand : ICommand<Result<NewsletterSubscriptionResponse>>
{
    [Required, MaxLength(200)] public string Email { get; init; } = string.Empty;
    [MaxLength(100)] public string? Source { get; init; }
}

public sealed record GetNewsletterSubscriptionsQuery : IQuery<Result<PagedResult<NewsletterSubscriptionResponse>>>
{
    public string? Search { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record DeleteNewsletterSubscriptionCommand(Guid Id) : ICommand<Result>;

public sealed class SubscribeNewsletterCommandHandler
    : IRequestHandler<SubscribeNewsletterCommand, Result<NewsletterSubscriptionResponse>>
{
    public Task<Result<NewsletterSubscriptionResponse>> Handle(
        SubscribeNewsletterCommand request, CancellationToken cancellationToken)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || email.Length > 200 ||
            !System.Text.RegularExpressions.Regex.IsMatch(
                email, @"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$"))
        {
            return Task.FromResult(Result.Failure<NewsletterSubscriptionResponse>(
                Error.Validation("newsletter.invalid_email", "Please enter a valid email address.")));
        }

        var existing = AdminCrudStore<NewsletterSubscriptionResponse>.All()
            .FirstOrDefault(s => string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return Task.FromResult(Result.Success(existing));
        }

        var response = new NewsletterSubscriptionResponse
        {
            Id = Guid.NewGuid(),
            Email = email,
            Source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source!.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
        };
        AdminCrudStore<NewsletterSubscriptionResponse>.Put(response);
        return Task.FromResult(Result.Success(response));
    }
}

public sealed class GetNewsletterSubscriptionsQueryHandler
    : IQueryHandler<GetNewsletterSubscriptionsQuery, Result<PagedResult<NewsletterSubscriptionResponse>>>
{
    public Task<Result<PagedResult<NewsletterSubscriptionResponse>>> Handle(
        GetNewsletterSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var filtered = AdminCrudStore<NewsletterSubscriptionResponse>.All()
            .Where(s => AdminPaging.Matches(request.Search, s.Email, s.Source))
            .OrderByDescending(s => s.CreatedAt);

        return Task.FromResult(Result.Success(AdminPaging.ToPaged(filtered, request.Page, request.PageSize)));
    }
}

public sealed class DeleteNewsletterSubscriptionCommandHandler
    : IRequestHandler<DeleteNewsletterSubscriptionCommand, Result>
{
    public Task<Result> Handle(
        DeleteNewsletterSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (!AdminCrudStore<NewsletterSubscriptionResponse>.Remove(request.Id))
        {
            return Task.FromResult(Result.Failure(AdminErrors.NotFound("Subscription", request.Id)));
        }

        return Task.FromResult(Result.Success());
    }
}
