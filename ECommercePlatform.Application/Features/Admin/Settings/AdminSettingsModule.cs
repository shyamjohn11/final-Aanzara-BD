using System.Text.Json;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Settings;

// IDs #143-144 — GET /api/admin/settings (load) + PUT /api/admin/settings (save).
// Persisted in SQL Server (AppSettings table, one row per key, values as JSON
// text). The previous in-memory defaults are seeded into the table on first
// read so the controlled form keeps its initial shape.

public sealed record GetSettingsQuery : IQuery<Result<Dictionary<string, JsonElement>>>;

public sealed record UpdateSettingsCommand(Dictionary<string, JsonElement> Values)
    : ICommand<Result<Dictionary<string, JsonElement>>>;

public sealed class GetSettingsQueryHandler
    : IQueryHandler<GetSettingsQuery, Result<Dictionary<string, JsonElement>>>
{
    private readonly IAdminRepository<AppSetting> _settings;
    private readonly IUnitOfWork _unitOfWork;

    public GetSettingsQueryHandler(IAdminRepository<AppSetting> settings, IUnitOfWork unitOfWork)
    {
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Dictionary<string, JsonElement>>> Handle(
        GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _settings.ListAsync(
            _ => true, q => q.OrderBy(s => s.Key), cancellationToken);

        if (rows.Count == 0)
        {
            foreach (var (key, raw) in SettingsDefaults.All)
            {
                _settings.Add(new AppSetting
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = raw
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            rows = await _settings.ListAsync(
                _ => true, q => q.OrderBy(s => s.Key), cancellationToken);
        }

        return Result.Success(SettingsDefaults.ToPayload(rows));
    }
}

public sealed class UpdateSettingsCommandHandler
    : IRequestHandler<UpdateSettingsCommand, Result<Dictionary<string, JsonElement>>>
{
    private readonly IAdminRepository<AppSetting> _settings;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSettingsCommandHandler(
        IAdminRepository<AppSetting> settings, IUnitOfWork unitOfWork)
    {
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Dictionary<string, JsonElement>>> Handle(
        UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        var rows = await _settings.ListAsync(
            _ => true, q => q.OrderBy(s => s.Key), cancellationToken);
        var byKey = rows.ToDictionary(r => r.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var (key, element) in request.Values ?? new())
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            var raw = element.ValueKind == JsonValueKind.Undefined ? "null" : element.GetRawText();

            if (byKey.TryGetValue(key, out var existing))
            {
                existing.Value = raw;
            }
            else
            {
                var row = new AppSetting { Id = Guid.NewGuid(), Key = key, Value = raw };
                _settings.Add(row);
                byKey[key] = row;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        rows = await _settings.ListAsync(
            _ => true, q => q.OrderBy(s => s.Key), cancellationToken);

        return Result.Success(SettingsDefaults.ToPayload(rows));
    }
}

internal static class SettingsDefaults
{
    internal static readonly IReadOnlyList<(string Key, string Raw)> All =
    [
        ("storeName", "\"Fresh Mart\""),
        ("supportEmail", "\"support@freshmart.in\""),
        ("currency", "\"INR\""),
        ("deliveryCharge", "40"),
        ("freeDeliveryAbove", "999"),
        ("maintenanceMode", "false"),
    ];

    internal static Dictionary<string, JsonElement> ToPayload(IEnumerable<AppSetting> rows)
    {
        var payload = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            try
            {
                using var document = JsonDocument.Parse(
                    string.IsNullOrWhiteSpace(row.Value) ? "null" : row.Value);
                payload[row.Key] = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                payload[row.Key] = JsonSerializer.SerializeToElement(row.Value);
            }
        }

        return payload;
    }
}
