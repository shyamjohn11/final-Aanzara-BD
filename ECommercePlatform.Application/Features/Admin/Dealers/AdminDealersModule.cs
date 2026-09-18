using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Dealers;

// IDs #150-155 — GET list / GET by id / POST / PUT / DELETE / PATCH status,
// Admin-role. Persisted in SQL Server (Dealers table); every dealer belongs
// to exactly one Agent (Agent 1 ── * Dealer).

public sealed record DealerResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public Guid DealerId { get; init; }
    public Guid AgentId { get; init; }
    public string? AgentName { get; init; }
    public string DealerCode { get; init; } = string.Empty;
    public string ShopName { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string? AlternatePhone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? Country { get; init; }
    public string? Pincode { get; init; }
    public string? GSTNumber { get; init; }
    public string? PANNumber { get; init; }
    public string? ShopDescription { get; init; }
    public string? ShopLogo { get; init; }
    public string Status { get; init; } = nameof(DealerStatus.Active);
    public int ProductCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetDealersQuery : IQuery<Result<PagedResult<DealerResponse>>>
{
    public Guid? AgentId { get; init; }
    public string? Search { get; init; }
    public string? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetDealerByIdQuery(Guid Id) : IQuery<Result<DealerResponse>>;

public sealed record CreateDealerCommand : ICommand<Result<DealerResponse>>
{
    public Guid AgentId { get; init; }

    [MaxLength(50)] public string? DealerCode { get; init; }

    [Required]
    [MaxLength(200)]
    public string ShopName { get; init; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string OwnerName { get; init; } = string.Empty;

    [EmailAddress]
    [MaxLength(320)]
    public string? Email { get; init; }

    [Required]
    [MaxLength(30)]
    public string Phone { get; init; } = string.Empty;

    [MaxLength(30)] public string? AlternatePhone { get; init; }
    [MaxLength(1000)] public string? Address { get; init; }
    [MaxLength(100)] public string? City { get; init; }
    [MaxLength(100)] public string? State { get; init; }
    [MaxLength(100)] public string? Country { get; init; }
    [MaxLength(20)] public string? Pincode { get; init; }
    [MaxLength(30)] public string? GSTNumber { get; init; }
    [MaxLength(20)] public string? PANNumber { get; init; }
    public string? ShopDescription { get; init; }
    [MaxLength(1000)] public string? ShopLogo { get; init; }
    [MaxLength(20)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateDealerCommand : ICommand<Result<DealerResponse>>
{
    [JsonIgnore]
    public Guid Id { get; init; }

    public Guid? AgentId { get; init; }

    [MaxLength(50)] public string? DealerCode { get; init; }
    [MaxLength(200)] public string? ShopName { get; init; }
    [MaxLength(150)] public string? OwnerName { get; init; }

    [EmailAddress]
    [MaxLength(320)]
    public string? Email { get; init; }

    [MaxLength(30)] public string? Phone { get; init; }
    [MaxLength(30)] public string? AlternatePhone { get; init; }
    [MaxLength(1000)] public string? Address { get; init; }
    [MaxLength(100)] public string? City { get; init; }
    [MaxLength(100)] public string? State { get; init; }
    [MaxLength(100)] public string? Country { get; init; }
    [MaxLength(20)] public string? Pincode { get; init; }
    [MaxLength(30)] public string? GSTNumber { get; init; }
    [MaxLength(20)] public string? PANNumber { get; init; }
    public string? ShopDescription { get; init; }
    [MaxLength(1000)] public string? ShopLogo { get; init; }
    [MaxLength(20)] public string? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteDealerCommand(Guid Id) : ICommand<Result>;

public sealed record UpdateDealerStatusCommand(Guid Id, string Status) : ICommand<Result<DealerResponse>>;

internal static class DealerErrors
{
    public static Error AgentNotFound(Guid agentId) => Error.NotFound(
        "admin.dealer_agent_not_found", $"Agent '{agentId}' was not found.");

    public static Error AgentNotActive(string agentName) => Error.Validation(
        "admin.dealer_agent_not_active", $"Agent '{agentName}' is not active.");

    public static Error DealerCodeTaken(string code) => Error.Conflict(
        "admin.dealer_code_taken", $"Dealer code '{code}' is already in use.");

    public static Error InvalidDealerStatus(string status) => Error.Validation(
        "admin.dealer_invalid_status", $"Status '{status}' is not a valid dealer status.");

    public static Error DealerHasProducts(int count) => Error.Conflict(
        "admin.dealer_has_products",
        $"This dealer owns {count} product(s). Move or remove them before deleting the dealer.");
}

internal static class DealerMappings
{
    internal static DealerResponse ToDto(Dealer dealer, string? agentName, int productCount) => new()
    {
        Id = dealer.Id,
        DealerId = dealer.Id,
        AgentId = dealer.AgentId,
        AgentName = agentName,
        DealerCode = dealer.DealerCode,
        ShopName = dealer.ShopName,
        OwnerName = dealer.OwnerName,
        Email = dealer.Email,
        Phone = dealer.Phone,
        AlternatePhone = dealer.AlternatePhone,
        Address = dealer.Address,
        City = dealer.City,
        State = dealer.State,
        Country = dealer.Country,
        Pincode = dealer.Pincode,
        GSTNumber = dealer.GSTNumber,
        PANNumber = dealer.PANNumber,
        ShopDescription = dealer.ShopDescription,
        ShopLogo = dealer.ShopLogo,
        Status = dealer.Status.ToString(),
        ProductCount = productCount,
        CreatedAt = dealer.CreatedAt,
        UpdatedAt = dealer.UpdatedAt
    };
}

public sealed class GetDealersQueryHandler(
    IAdminRepository<Dealer> dealers,
    IAgentRepository agents,
    IProductRepository products)
    : IQueryHandler<GetDealersQuery, Result<PagedResult<DealerResponse>>>
{
    public async Task<Result<PagedResult<DealerResponse>>> Handle(
        GetDealersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        DealerStatus? status = null;
        if (request.Status is { } rawStatus)
        {
            if (!Enum.TryParse<DealerStatus>(rawStatus.Trim(), ignoreCase: true, out var parsed))
            {
                return Result.Failure<PagedResult<DealerResponse>>(
                    DealerErrors.InvalidDealerStatus(rawStatus));
            }

            status = parsed;
        }

        Expression<Func<Dealer, bool>> filter = AdminFilters.True<Dealer>();

        if (request.AgentId.HasValue)
        {
            var agentId = request.AgentId.Value;
            filter = filter.And(d => d.AgentId == agentId);
        }

        if (status.HasValue)
        {
            var wanted = status.Value;
            filter = filter.And(d => d.Status == wanted);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search;
            filter = filter.And(d =>
                d.ShopName.Contains(s) ||
                d.OwnerName.Contains(s) ||
                d.DealerCode.Contains(s) ||
                (d.City != null && d.City.Contains(s)) ||
                (d.Phone != null && d.Phone.Contains(s)));
        }

        var total = await dealers.CountAsync(filter, cancellationToken);
        var rows = await dealers.PageAsync(
            filter,
            q => q.OrderByDescending(d => d.CreatedAt),
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        // One batch lookup for agent names + one count per row (admin scale).
        var agentIds = rows.Select(d => d.AgentId).Distinct().ToArray();
        var agentNames = (await agents.GetByIdsAsync(agentIds, cancellationToken))
            .ToDictionary(a => a.AgentId, a => a.User.Name);

        var items = new List<DealerResponse>(rows.Count);
        foreach (var dealer in rows)
        {
            agentNames.TryGetValue(dealer.AgentId, out var agentName);
            var productCount = await products.CountByDealerAsync(dealer.Id, cancellationToken);
            items.Add(DealerMappings.ToDto(dealer, agentName, productCount));
        }

        return Result.Success(new PagedResult<DealerResponse>(items, page, pageSize, total));
    }
}

public sealed class GetDealerByIdQueryHandler(
    IAdminRepository<Dealer> dealers,
    IAgentRepository agents,
    IProductRepository products)
    : IQueryHandler<GetDealerByIdQuery, Result<DealerResponse>>
{
    public async Task<Result<DealerResponse>> Handle(
        GetDealerByIdQuery request, CancellationToken cancellationToken)
    {
        var dealer = await dealers.GetByIdAsync(request.Id, cancellationToken);
        if (dealer is null)
        {
            return Result.Failure<DealerResponse>(AdminErrors.NotFound("Dealer", request.Id));
        }

        var agent = await agents.GetByIdAsync(dealer.AgentId, cancellationToken);
        var productCount = await products.CountByDealerAsync(dealer.Id, cancellationToken);

        return Result.Success(DealerMappings.ToDto(dealer, agent?.User.Name, productCount));
    }
}

public sealed class CreateDealerCommandHandler(
    IAdminRepository<Dealer> dealers,
    IAgentRepository agents,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateDealerCommand, Result<DealerResponse>>
{
    public async Task<Result<DealerResponse>> Handle(
        CreateDealerCommand request, CancellationToken cancellationToken)
    {
        var agent = await agents.GetByIdAsync(request.AgentId, cancellationToken);
        if (agent is null)
        {
            return Result.Failure<DealerResponse>(DealerErrors.AgentNotFound(request.AgentId));
        }

        if (agent.Status != AgentStatus.Active)
        {
            return Result.Failure<DealerResponse>(DealerErrors.AgentNotActive(agent.User.Name));
        }

        if (!Enum.TryParse<DealerStatus>(request.Status?.Trim() ?? nameof(DealerStatus.Active),
                ignoreCase: true, out var status))
        {
            return Result.Failure<DealerResponse>(DealerErrors.InvalidDealerStatus(request.Status ?? string.Empty));
        }

        var code = request.DealerCode?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await GenerateDealerCodeAsync(cancellationToken);
        }

        if (await dealers.CountAsync(d => d.DealerCode == code, cancellationToken) > 0)
        {
            return Result.Failure<DealerResponse>(DealerErrors.DealerCodeTaken(code));
        }

        var now = timeProvider.GetUtcNow();
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            AgentId = request.AgentId,
            DealerCode = code,
            ShopName = request.ShopName.Trim(),
            OwnerName = request.OwnerName.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Phone = request.Phone.Trim(),
            AlternatePhone = string.IsNullOrWhiteSpace(request.AlternatePhone) ? null : request.AlternatePhone.Trim(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            City = request.City?.Trim(),
            State = request.State?.Trim(),
            Country = request.Country?.Trim(),
            Pincode = request.Pincode?.Trim(),
            GSTNumber = request.GSTNumber?.Trim(),
            PANNumber = request.PANNumber?.Trim(),
            ShopDescription = string.IsNullOrWhiteSpace(request.ShopDescription) ? null : request.ShopDescription.Trim(),
            ShopLogo = request.ShopLogo?.Trim(),
            Status = status,
            CreatedAt = now,
            UpdatedAt = now
        };

        dealers.Add(dealer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(DealerMappings.ToDto(dealer, agent.User.Name, 0));
    }

    private async Task<string> GenerateDealerCodeAsync(CancellationToken cancellationToken)
    {
        // DLR-XXXXXX is unique-enough that one retry loop is plenty.
        while (true)
        {
            var code = "DLR-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            if (await dealers.CountAsync(d => d.DealerCode == code, cancellationToken) == 0)
            {
                return code;
            }
        }
    }
}

public sealed class UpdateDealerCommandHandler(
    IAdminRepository<Dealer> dealers,
    IAgentRepository agents,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDealerCommand, Result<DealerResponse>>
{
    public async Task<Result<DealerResponse>> Handle(
        UpdateDealerCommand request, CancellationToken cancellationToken)
    {
        var dealer = await dealers.GetByIdAsync(request.Id, cancellationToken);
        if (dealer is null)
        {
            return Result.Failure<DealerResponse>(AdminErrors.NotFound("Dealer", request.Id));
        }

        // Agent reassignment is allowed but always re-validated server-side.
        if (request.AgentId.HasValue && request.AgentId.Value != dealer.AgentId)
        {
            var agent = await agents.GetByIdAsync(request.AgentId.Value, cancellationToken);
            if (agent is null)
            {
                return Result.Failure<DealerResponse>(DealerErrors.AgentNotFound(request.AgentId.Value));
            }

            if (agent.Status != AgentStatus.Active)
            {
                return Result.Failure<DealerResponse>(DealerErrors.AgentNotActive(agent.User.Name));
            }

            dealer.AgentId = agent.AgentId;
        }

        if (request.DealerCode is { } rawCode && !string.Equals(rawCode.Trim(), dealer.DealerCode, StringComparison.Ordinal))
        {
            var code = rawCode.Trim();
            if (await dealers.CountAsync(
                    d => d.DealerCode == code && d.Id != dealer.Id, cancellationToken) > 0)
            {
                return Result.Failure<DealerResponse>(DealerErrors.DealerCodeTaken(code));
            }

            dealer.DealerCode = code;
        }

        if (request.Status is { } rawStatus)
        {
            if (!Enum.TryParse<DealerStatus>(rawStatus.Trim(), ignoreCase: true, out var status))
            {
                return Result.Failure<DealerResponse>(DealerErrors.InvalidDealerStatus(rawStatus));
            }

            dealer.Status = status;
        }

        if (request.ShopName is { } shopName) dealer.ShopName = shopName.Trim();
        if (request.OwnerName is { } ownerName) dealer.OwnerName = ownerName.Trim();
        if (request.Email is { } email) dealer.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        if (request.Phone is { } phone) dealer.Phone = phone.Trim();
        if (request.AlternatePhone is { } alternatePhone) dealer.AlternatePhone = string.IsNullOrWhiteSpace(alternatePhone) ? null : alternatePhone.Trim();
        if (request.Address is { } address) dealer.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        if (request.City is { } city) dealer.City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        if (request.State is { } state) dealer.State = string.IsNullOrWhiteSpace(state) ? null : state.Trim();
        if (request.Country is { } country) dealer.Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim();
        if (request.Pincode is { } pincode) dealer.Pincode = string.IsNullOrWhiteSpace(pincode) ? null : pincode.Trim();
        if (request.GSTNumber is { } gst) dealer.GSTNumber = string.IsNullOrWhiteSpace(gst) ? null : gst.Trim();
        if (request.PANNumber is { } pan) dealer.PANNumber = string.IsNullOrWhiteSpace(pan) ? null : pan.Trim();
        if (request.ShopDescription is { } description) dealer.ShopDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (request.ShopLogo is { } logo) dealer.ShopLogo = string.IsNullOrWhiteSpace(logo) ? null : logo.Trim();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var owner = await agents.GetByIdAsync(dealer.AgentId, cancellationToken);
        return Result.Success(DealerMappings.ToDto(dealer, owner?.User.Name, 0));
    }
}

public sealed class DeleteDealerCommandHandler(
    IAdminRepository<Dealer> dealers,
    IProductRepository products,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteDealerCommand, Result>
{
    public async Task<Result> Handle(DeleteDealerCommand request, CancellationToken cancellationToken)
    {
        var dealer = await dealers.GetByIdAsync(request.Id, cancellationToken);
        if (dealer is null)
        {
            return Result.Failure(AdminErrors.NotFound("Dealer", request.Id));
        }

        // Historical order rows snapshot ProductId+price, so deleting the dealer
        // is safe once its products are gone — but never with products attached.
        var productCount = await products.CountByDealerAsync(dealer.Id, cancellationToken);
        if (productCount > 0)
        {
            return Result.Failure(DealerErrors.DealerHasProducts(productCount));
        }

        dealers.Remove(dealer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class UpdateDealerStatusCommandHandler(
    IAdminRepository<Dealer> dealers,
    IAgentRepository agents,
    IProductRepository products,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDealerStatusCommand, Result<DealerResponse>>
{
    public async Task<Result<DealerResponse>> Handle(
        UpdateDealerStatusCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Result.Failure<DealerResponse>(
                Error.Validation("admin.status_required", "Status is required."));
        }

        if (!Enum.TryParse<DealerStatus>(request.Status.Trim(), ignoreCase: true, out var status))
        {
            return Result.Failure<DealerResponse>(DealerErrors.InvalidDealerStatus(request.Status));
        }

        var dealer = await dealers.GetByIdAsync(request.Id, cancellationToken);
        if (dealer is null)
        {
            return Result.Failure<DealerResponse>(AdminErrors.NotFound("Dealer", request.Id));
        }

        dealer.Status = status;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var agent = await agents.GetByIdAsync(dealer.AgentId, cancellationToken);
        var productCount = await products.CountByDealerAsync(dealer.Id, cancellationToken);

        return Result.Success(DealerMappings.ToDto(dealer, agent?.User.Name, productCount));
    }
}
