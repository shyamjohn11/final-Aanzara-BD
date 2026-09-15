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

namespace ECommercePlatform.Application.Features.Admin.Quotes;

// IDs #122-126 — GET list / GET by id / POST / PUT / DELETE, Admin-role.
// Persisted in SQL Server (Quotes table). Wire shape unchanged.

public sealed record QuoteResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string Product { get; init; } = string.Empty;
    public int Quantity { get; init; } = 1;
    public string? Message { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetQuotesQuery : IQuery<Result<PagedResult<QuoteResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetQuoteByIdQuery(Guid Id) : IQuery<Result<QuoteResponse>>;

public sealed record CreateQuoteCommand : ICommand<Result<QuoteResponse>>
{
    [MaxLength(200)] public string? CustomerName { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(300)] public string? Product { get; init; }
    [Range(1, 1000000)] public int Quantity { get; init; } = 1;
    [MaxLength(4000)] public string? Message { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateQuoteCommand : ICommand<Result<QuoteResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(200)] public string? CustomerName { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(300)] public string? Product { get; init; }
    public int? Quantity { get; init; }
    [MaxLength(4000)] public string? Message { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteQuoteCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateQuoteStatusCommand(Guid Id, string Status)
    : ICommand<Result<QuoteResponse>>;

internal static class QuoteMappings
{
    internal static QuoteResponse ToDto(this Quote quote) => new()
    {
        Id = quote.Id,
        CustomerName = quote.CustomerName,
        Email = quote.Email,
        Phone = quote.Phone,
        Product = quote.Product,
        Quantity = quote.Quantity,
        Message = quote.Message,
        Status = quote.Status,
        CreatedAt = quote.CreatedAt,
        UpdatedAt = quote.UpdatedAt
    };
}

public sealed class GetQuotesQueryHandler
    : IQueryHandler<GetQuotesQuery, Result<PagedResult<QuoteResponse>>>
{
    private readonly IAdminRepository<Quote> _quotes;

    public GetQuotesQueryHandler(IAdminRepository<Quote> quotes) => _quotes = quotes;

    public async Task<Result<PagedResult<QuoteResponse>>> Handle(
        GetQuotesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<Quote, bool>> filter = AdminFilters.True<Quote>();

        var status = request.Status?.Trim();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = filter.And(q => q.Status == status);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(q =>
                q.CustomerName.Contains(s)
                || (q.Email != null && q.Email.Contains(s))
                || (q.Phone != null && q.Phone.Contains(s))
                || q.Product.Contains(s)
                || (q.Message != null && q.Message.Contains(s)));
        }

        var total = await _quotes.CountAsync(filter, cancellationToken);
        var items = await _quotes.PageAsync(
            filter,
            q => q.OrderByDescending(x => x.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<QuoteResponse>(
            items.Select(q => q.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class GetQuoteByIdQueryHandler
    : IQueryHandler<GetQuoteByIdQuery, Result<QuoteResponse>>
{
    private readonly IAdminRepository<Quote> _quotes;

    public GetQuoteByIdQueryHandler(IAdminRepository<Quote> quotes) => _quotes = quotes;

    public async Task<Result<QuoteResponse>> Handle(
        GetQuoteByIdQuery request, CancellationToken cancellationToken)
    {
        var quote = await _quotes.GetByIdAsync(request.Id, cancellationToken);

        return quote is null
            ? Result.Failure<QuoteResponse>(AdminErrors.NotFound("Quote", request.Id))
            : Result.Success(quote.ToDto());
    }
}

public sealed class CreateQuoteCommandHandler
    : IRequestHandler<CreateQuoteCommand, Result<QuoteResponse>>
{
    private readonly IAdminRepository<Quote> _quotes;
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public CreateQuoteCommandHandler(
        IAdminRepository<Quote> quotes,
        IAdminRepository<Notification> notifications,
        IUnitOfWork unitOfWork)
    {
        _quotes = quotes;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<QuoteResponse>> Handle(
        CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName ?? request.Name ?? "Customer",
            Email = request.Email,
            Phone = request.Phone,
            Product = request.Product ?? "Product",
            Quantity = request.Quantity <= 0 ? 1 : request.Quantity,
            Message = request.Message,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status!
        };

        _quotes.Add(quote);
        NotificationEmitter.Emit(
            _notifications,
            "quote",
            $"New quote request: {quote.Product} x {quote.Quantity}",
            $"From {quote.CustomerName}.",
            "/admin/quotes");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(quote.ToDto());
    }
}

public sealed class UpdateQuoteCommandHandler
    : IRequestHandler<UpdateQuoteCommand, Result<QuoteResponse>>
{
    private readonly IAdminRepository<Quote> _quotes;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuoteCommandHandler(IAdminRepository<Quote> quotes, IUnitOfWork unitOfWork)
    {
        _quotes = quotes;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<QuoteResponse>> Handle(
        UpdateQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await _quotes.GetByIdAsync(request.Id, cancellationToken);

        if (quote is null)
        {
            return Result.Failure<QuoteResponse>(AdminErrors.NotFound("Quote", request.Id));
        }

        quote.CustomerName = request.CustomerName ?? request.Name ?? quote.CustomerName;
        quote.Email = request.Email ?? quote.Email;
        quote.Phone = request.Phone ?? quote.Phone;
        quote.Product = request.Product ?? quote.Product;
        quote.Quantity = request.Quantity ?? quote.Quantity;
        quote.Message = request.Message ?? quote.Message;
        quote.Status = request.Status ?? quote.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(quote.ToDto());
    }
}

public sealed class DeleteQuoteCommandHandler : IRequestHandler<DeleteQuoteCommand, Result>
{
    private readonly IAdminRepository<Quote> _quotes;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteQuoteCommandHandler(IAdminRepository<Quote> quotes, IUnitOfWork unitOfWork)
    {
        _quotes = quotes;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await _quotes.GetByIdAsync(request.Id, cancellationToken);

        if (quote is null)
        {
            return Result.Failure(AdminErrors.NotFound("Quote", request.Id));
        }

        _quotes.Remove(quote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class UpdateQuoteStatusCommandHandler
    : IRequestHandler<UpdateQuoteStatusCommand, Result<QuoteResponse>>
{
    private readonly IAdminRepository<Quote> _quotes;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuoteStatusCommandHandler(IAdminRepository<Quote> quotes, IUnitOfWork unitOfWork)
    {
        _quotes = quotes;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<QuoteResponse>> Handle(
        UpdateQuoteStatusCommand request, CancellationToken cancellationToken)
    {
        var quote = await _quotes.GetByIdAsync(request.Id, cancellationToken);

        if (quote is null)
        {
            return Result.Failure<QuoteResponse>(AdminErrors.NotFound("Quote", request.Id));
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Result.Failure<QuoteResponse>(
                Error.Validation("admin.status_required", "Status is required."));
        }

        quote.Status = request.Status.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(quote.ToDto());
    }
}
