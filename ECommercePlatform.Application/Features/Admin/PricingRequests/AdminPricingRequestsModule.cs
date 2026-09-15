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

namespace ECommercePlatform.Application.Features.Admin.PricingRequests;

// IDs #135-138 + POST/PUT (A3) — list, details, create, update,
// PATCH /{id}/status (approve/reject), delete.
// Persisted in SQL Server (PricingRequests table). Wire shape unchanged.

public sealed record PricingRequestResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string Product { get; init; } = string.Empty;
    public int Quantity { get; init; } = 1;
    public decimal RequestedPrice { get; init; }
    public string? Message { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetPricingRequestsQuery : IQuery<Result<PagedResult<PricingRequestResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetPricingRequestByIdQuery(Guid Id) : IQuery<Result<PricingRequestResponse>>;

public sealed record UpdatePricingRequestStatusCommand(Guid Id, string Status)
    : ICommand<Result<PricingRequestResponse>>;

public sealed record DeletePricingRequestCommand(Guid Id) : ICommand<Result>;

public sealed record CreatePricingRequestCommand : ICommand<Result<PricingRequestResponse>>
{
    [MaxLength(200)] public string? CustomerName { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(300)] public string? Product { get; init; }
    [Range(1, 1000000)] public int Quantity { get; init; } = 1;
    [Range(0, 100000000)] public decimal RequestedPrice { get; init; }
    [MaxLength(4000)] public string? Message { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdatePricingRequestCommand : ICommand<Result<PricingRequestResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(200)] public string? CustomerName { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(300)] public string? Product { get; init; }
    public int? Quantity { get; init; }
    public decimal? RequestedPrice { get; init; }
    [MaxLength(4000)] public string? Message { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

internal static class PricingRequestMappings
{
    internal static PricingRequestResponse ToDto(this PricingRequest request) => new()
    {
        Id = request.Id,
        CustomerName = request.CustomerName,
        Email = request.Email,
        Phone = request.Phone,
        Product = request.Product,
        Quantity = request.Quantity,
        RequestedPrice = request.RequestedPrice,
        Message = request.Message,
        Status = request.Status,
        CreatedAt = request.CreatedAt,
        UpdatedAt = request.UpdatedAt
    };
}

public sealed class GetPricingRequestsQueryHandler
    : IQueryHandler<GetPricingRequestsQuery, Result<PagedResult<PricingRequestResponse>>>
{
    private readonly IAdminRepository<PricingRequest> _requests;

    public GetPricingRequestsQueryHandler(IAdminRepository<PricingRequest> requests) => _requests = requests;

    public async Task<Result<PagedResult<PricingRequestResponse>>> Handle(
        GetPricingRequestsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<PricingRequest, bool>> filter = AdminFilters.True<PricingRequest>();

        var status = request.Status?.Trim();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = filter.And(p => p.Status == status);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(p =>
                p.CustomerName.Contains(s)
                || (p.Email != null && p.Email.Contains(s))
                || (p.Phone != null && p.Phone.Contains(s))
                || p.Product.Contains(s)
                || (p.Message != null && p.Message.Contains(s)));
        }

        var total = await _requests.CountAsync(filter, cancellationToken);
        var items = await _requests.PageAsync(
            filter,
            q => q.OrderByDescending(p => p.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<PricingRequestResponse>(
            items.Select(p => p.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class GetPricingRequestByIdQueryHandler
    : IQueryHandler<GetPricingRequestByIdQuery, Result<PricingRequestResponse>>
{
    private readonly IAdminRepository<PricingRequest> _requests;

    public GetPricingRequestByIdQueryHandler(IAdminRepository<PricingRequest> requests) => _requests = requests;

    public async Task<Result<PricingRequestResponse>> Handle(
        GetPricingRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var pricingRequest = await _requests.GetByIdAsync(request.Id, cancellationToken);

        return pricingRequest is null
            ? Result.Failure<PricingRequestResponse>(AdminErrors.NotFound("Pricing request", request.Id))
            : Result.Success(pricingRequest.ToDto());
    }
}

public sealed class UpdatePricingRequestStatusCommandHandler
    : IRequestHandler<UpdatePricingRequestStatusCommand, Result<PricingRequestResponse>>
{
    private readonly IAdminRepository<PricingRequest> _requests;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePricingRequestStatusCommandHandler(
        IAdminRepository<PricingRequest> requests, IUnitOfWork unitOfWork)
    {
        _requests = requests;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PricingRequestResponse>> Handle(
        UpdatePricingRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var pricingRequest = await _requests.GetByIdAsync(request.Id, cancellationToken);

        if (pricingRequest is null)
        {
            return Result.Failure<PricingRequestResponse>(AdminErrors.NotFound("Pricing request", request.Id));
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Result.Failure<PricingRequestResponse>(
                Error.Validation("admin.status_required", "Status is required."));
        }

        pricingRequest.Status = request.Status.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(pricingRequest.ToDto());
    }
}

public sealed class DeletePricingRequestCommandHandler
    : IRequestHandler<DeletePricingRequestCommand, Result>
{
    private readonly IAdminRepository<PricingRequest> _requests;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePricingRequestCommandHandler(
        IAdminRepository<PricingRequest> requests, IUnitOfWork unitOfWork)
    {
        _requests = requests;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeletePricingRequestCommand request, CancellationToken cancellationToken)
    {
        var pricingRequest = await _requests.GetByIdAsync(request.Id, cancellationToken);

        if (pricingRequest is null)
        {
            return Result.Failure(AdminErrors.NotFound("Pricing request", request.Id));
        }

        _requests.Remove(pricingRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class CreatePricingRequestCommandHandler
    : IRequestHandler<CreatePricingRequestCommand, Result<PricingRequestResponse>>
{
    private readonly IAdminRepository<PricingRequest> _requests;
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePricingRequestCommandHandler(
        IAdminRepository<PricingRequest> requests,
        IAdminRepository<Notification> notifications,
        IUnitOfWork unitOfWork)
    {
        _requests = requests;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PricingRequestResponse>> Handle(
        CreatePricingRequestCommand request, CancellationToken cancellationToken)
    {
        var pricingRequest = new PricingRequest
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName ?? request.Name ?? "Customer",
            Email = request.Email,
            Phone = request.Phone,
            Product = request.Product ?? "Product",
            Quantity = request.Quantity <= 0 ? 1 : request.Quantity,
            RequestedPrice = request.RequestedPrice,
            Message = request.Message,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status!
        };

        _requests.Add(pricingRequest);
        NotificationEmitter.Emit(
            _notifications,
            "pricing",
            $"New pricing request: {pricingRequest.Product} x {pricingRequest.Quantity}",
            $"From {pricingRequest.CustomerName}.",
            "/admin/pricing-requests");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(pricingRequest.ToDto());
    }
}

public sealed class UpdatePricingRequestCommandHandler
    : IRequestHandler<UpdatePricingRequestCommand, Result<PricingRequestResponse>>
{
    private readonly IAdminRepository<PricingRequest> _requests;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePricingRequestCommandHandler(
        IAdminRepository<PricingRequest> requests, IUnitOfWork unitOfWork)
    {
        _requests = requests;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PricingRequestResponse>> Handle(
        UpdatePricingRequestCommand request, CancellationToken cancellationToken)
    {
        var pricingRequest = await _requests.GetByIdAsync(request.Id, cancellationToken);

        if (pricingRequest is null)
        {
            return Result.Failure<PricingRequestResponse>(AdminErrors.NotFound("Pricing request", request.Id));
        }

        if (request.Quantity is <= 0)
        {
            return Result.Failure<PricingRequestResponse>(
                Error.Validation("admin.quantity_invalid", "Quantity must be greater than zero."));
        }

        if (request.RequestedPrice is < 0)
        {
            return Result.Failure<PricingRequestResponse>(
                Error.Validation("admin.price_invalid", "Requested price cannot be negative."));
        }

        pricingRequest.CustomerName = request.CustomerName ?? request.Name ?? pricingRequest.CustomerName;
        pricingRequest.Email = request.Email ?? pricingRequest.Email;
        pricingRequest.Phone = request.Phone ?? pricingRequest.Phone;
        pricingRequest.Product = request.Product ?? pricingRequest.Product;
        pricingRequest.Quantity = request.Quantity ?? pricingRequest.Quantity;
        pricingRequest.RequestedPrice = request.RequestedPrice ?? pricingRequest.RequestedPrice;
        pricingRequest.Message = request.Message ?? pricingRequest.Message;
        pricingRequest.Status = request.Status ?? pricingRequest.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(pricingRequest.ToDto());
    }
}
