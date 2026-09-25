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
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Admin.Enquiries;

// IDs #131-134 + POST/PUT (A2) — list, details, create, update,
// PATCH /{id}/status (resolve), delete.
// Persisted in SQL Server (Enquiries table). Wire shape unchanged.

public sealed record EnquiryResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string Status { get; init; } = "Open";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetEnquiriesQuery : IQuery<Result<PagedResult<EnquiryResponse>>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetEnquiryByIdQuery(Guid Id) : IQuery<Result<EnquiryResponse>>;

public sealed record UpdateEnquiryStatusCommand(Guid Id, string Status)
    : ICommand<Result<EnquiryResponse>>;

public sealed record DeleteEnquiryCommand(Guid Id) : ICommand<Result>;

public sealed record CreateEnquiryCommand : ICommand<Result<EnquiryResponse>>
{
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(300)] public string? Subject { get; init; }
    [MaxLength(4000)] public string? Message { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateEnquiryCommand : ICommand<Result<EnquiryResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(320)] public string? Email { get; init; }
    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(300)] public string? Subject { get; init; }
    [MaxLength(4000)] public string? Message { get; init; }
    [MaxLength(50)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

internal static class EnquiryMappings
{
    internal static EnquiryResponse ToDto(this Enquiry enquiry) => new()
    {
        Id = enquiry.Id,
        Name = enquiry.Name,
        Email = enquiry.Email,
        Phone = enquiry.Phone,
        Subject = enquiry.Subject,
        Message = enquiry.Message,
        Status = enquiry.Status,
        CreatedAt = enquiry.CreatedAt,
        UpdatedAt = enquiry.UpdatedAt
    };
}

public sealed class GetEnquiriesQueryHandler
    : IQueryHandler<GetEnquiriesQuery, Result<PagedResult<EnquiryResponse>>>
{
    private readonly IAdminRepository<Enquiry> _enquiries;

    public GetEnquiriesQueryHandler(IAdminRepository<Enquiry> enquiries) => _enquiries = enquiries;

    public async Task<Result<PagedResult<EnquiryResponse>>> Handle(
        GetEnquiriesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        Expression<Func<Enquiry, bool>> filter = AdminFilters.True<Enquiry>();

        var status = request.Status?.Trim();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = filter.And(e => e.Status == status);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(e =>
                e.Name.Contains(s)
                || (e.Email != null && e.Email.Contains(s))
                || (e.Phone != null && e.Phone.Contains(s))
                || e.Subject.Contains(s)
                || (e.Message != null && e.Message.Contains(s)));
        }

        var total = await _enquiries.CountAsync(filter, cancellationToken);
        var items = await _enquiries.PageAsync(
            filter,
            q => q.OrderByDescending(e => e.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return Result.Success(new PagedResult<EnquiryResponse>(
            items.Select(e => e.ToDto()).ToArray(), page, pageSize, total));
    }
}

public sealed class GetEnquiryByIdQueryHandler
    : IQueryHandler<GetEnquiryByIdQuery, Result<EnquiryResponse>>
{
    private readonly IAdminRepository<Enquiry> _enquiries;

    public GetEnquiryByIdQueryHandler(IAdminRepository<Enquiry> enquiries) => _enquiries = enquiries;

    public async Task<Result<EnquiryResponse>> Handle(
        GetEnquiryByIdQuery request, CancellationToken cancellationToken)
    {
        var enquiry = await _enquiries.GetByIdAsync(request.Id, cancellationToken);

        return enquiry is null
            ? Result.Failure<EnquiryResponse>(AdminErrors.NotFound("Enquiry", request.Id))
            : Result.Success(enquiry.ToDto());
    }
}

public sealed class UpdateEnquiryStatusCommandHandler
    : IRequestHandler<UpdateEnquiryStatusCommand, Result<EnquiryResponse>>
{
    private readonly IAdminRepository<Enquiry> _enquiries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpdateEnquiryStatusCommandHandler> _logger;

    public UpdateEnquiryStatusCommandHandler(
        IAdminRepository<Enquiry> enquiries,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<UpdateEnquiryStatusCommandHandler> logger)
    {
        _enquiries = enquiries;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<EnquiryResponse>> Handle(
        UpdateEnquiryStatusCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await _enquiries.GetByIdAsync(request.Id, cancellationToken);

        if (enquiry is null)
        {
            return Result.Failure<EnquiryResponse>(AdminErrors.NotFound("Enquiry", request.Id));
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Result.Failure<EnquiryResponse>(
                Error.Validation("admin.status_required", "Status is required."));
        }

        var previous = enquiry.Status;
        enquiry.Status = request.Status.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Best-effort user email on real status changes; failure must never
        // fail the update. Unchanged statuses stay silent (no spam on re-save).
        if (!string.Equals(previous, enquiry.Status, StringComparison.OrdinalIgnoreCase))
        {
            await RequestStatusEmail.TrySendAsync(
                _emailService,
                _logger,
                enquiry.Email,
                $"Your enquiry is now {enquiry.Status}",
                RequestStatusEmail.EnquiryHtml(enquiry.Name, enquiry.Subject, enquiry.Status),
                cancellationToken);
        }

        return Result.Success(enquiry.ToDto());
    }
}

public sealed class DeleteEnquiryCommandHandler : IRequestHandler<DeleteEnquiryCommand, Result>
{
    private readonly IAdminRepository<Enquiry> _enquiries;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEnquiryCommandHandler(IAdminRepository<Enquiry> enquiries, IUnitOfWork unitOfWork)
    {
        _enquiries = enquiries;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await _enquiries.GetByIdAsync(request.Id, cancellationToken);

        if (enquiry is null)
        {
            return Result.Failure(AdminErrors.NotFound("Enquiry", request.Id));
        }

        _enquiries.Remove(enquiry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class CreateEnquiryCommandHandler
    : IRequestHandler<CreateEnquiryCommand, Result<EnquiryResponse>>
{
    private readonly IAdminRepository<Enquiry> _enquiries;
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEnquiryCommandHandler(
        IAdminRepository<Enquiry> enquiries,
        IAdminRepository<Notification> notifications,
        IUnitOfWork unitOfWork)
    {
        _enquiries = enquiries;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EnquiryResponse>> Handle(
        CreateEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = new Enquiry
        {
            Id = Guid.NewGuid(),
            Name = request.Name ?? "Customer",
            Email = request.Email,
            Phone = request.Phone,
            Subject = request.Subject ?? "General enquiry",
            Message = request.Message,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Open" : request.Status!
        };

        _enquiries.Add(enquiry);
        NotificationEmitter.Emit(
            _notifications,
            "enquiry",
            $"New enquiry: {enquiry.Subject}",
            string.IsNullOrWhiteSpace(enquiry.Message)
                ? $"From {enquiry.Name} ({enquiry.Email})."
                : $"{enquiry.Name}: {enquiry.Message}",
            "/admin/enquiries");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(enquiry.ToDto());
    }
}

public sealed class UpdateEnquiryCommandHandler
    : IRequestHandler<UpdateEnquiryCommand, Result<EnquiryResponse>>
{
    private readonly IAdminRepository<Enquiry> _enquiries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpdateEnquiryCommandHandler> _logger;

    public UpdateEnquiryCommandHandler(
        IAdminRepository<Enquiry> enquiries,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<UpdateEnquiryCommandHandler> logger)
    {
        _enquiries = enquiries;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<EnquiryResponse>> Handle(
        UpdateEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await _enquiries.GetByIdAsync(request.Id, cancellationToken);

        if (enquiry is null)
        {
            return Result.Failure<EnquiryResponse>(AdminErrors.NotFound("Enquiry", request.Id));
        }

        var previous = enquiry.Status;

        enquiry.Name = request.Name ?? enquiry.Name;
        enquiry.Email = request.Email ?? enquiry.Email;
        enquiry.Phone = request.Phone ?? enquiry.Phone;
        enquiry.Subject = request.Subject ?? enquiry.Subject;
        enquiry.Message = request.Message ?? enquiry.Message;
        enquiry.Status = request.Status ?? enquiry.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Best-effort user email when the edit changed the status.
        if (request.Status is not null
            && !string.Equals(previous, enquiry.Status, StringComparison.OrdinalIgnoreCase))
        {
            await RequestStatusEmail.TrySendAsync(
                _emailService,
                _logger,
                enquiry.Email,
                $"Your enquiry is now {enquiry.Status}",
                RequestStatusEmail.EnquiryHtml(enquiry.Name, enquiry.Subject, enquiry.Status),
                cancellationToken);
        }

        return Result.Success(enquiry.ToDto());
    }
}
