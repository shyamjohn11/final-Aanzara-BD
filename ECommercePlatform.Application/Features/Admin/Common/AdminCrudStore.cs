using System.Collections.Concurrent;
using System.Text.Json;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Common;

/// <summary>
/// Marker for admin DTOs stored in the additive in-memory stores.
/// New admin modules (#69-147) use this so they never touch existing
/// DbContext repositories or migrations.
/// </summary>
public interface IAdminEntity
{
    Guid Id { get; }
}

/// <summary>
/// Thread-safe in-memory CRUD backing for admin CMS-style resources
/// (banners, offers, combos, ...). Seeded lazily with mock-shaped data so a
/// page never goes blank; purely additive — no existing module is modified.
/// </summary>
public static class AdminCrudStore<T>
    where T : class, IAdminEntity
{
    private static readonly ConcurrentDictionary<Guid, T> Items = new();
    private static int _seeded;

    public static void EnsureSeeded(Func<IReadOnlyList<T>> factory)
    {
        if (Interlocked.CompareExchange(ref _seeded, 1, 0) != 0)
        {
            return;
        }

        foreach (var item in factory())
        {
            Items[item.Id] = item;
        }
    }

    public static IReadOnlyList<T> All() => Items.Values.ToList();

    public static bool TryGet(Guid id, out T? value) => Items.TryGetValue(id, out value);

    public static void Put(T value) => Items[value.Id] = value;

    public static bool Remove(Guid id) => Items.TryRemove(id, out _);

    public static void ClearForTests()
    {
        Items.Clear();
        Interlocked.Exchange(ref _seeded, 0);
    }
}

/// <summary>Shared paging + search helpers for the additive admin modules.</summary>
public static class AdminPaging
{
    public static PagedResult<T> ToPaged<T>(
        IEnumerable<T> source, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 25 : pageSize, 1, 200);

        var list = source.ToList();
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        return new PagedResult<T>(items, page, pageSize, list.Count);
    }

    public static bool Matches(string? search, params string?[] fields)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var s = search.Trim();

        return fields.Any(f =>
            !string.IsNullOrWhiteSpace(f) &&
            f.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    public static bool MatchesStatus(string? filter, string? status)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return true;
        }

        return string.Equals(filter.Trim(), status?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool MatchesExtra(string? search, Dictionary<string, JsonElement>? extra)
    {
        if (string.IsNullOrWhiteSpace(search) || extra is null || extra.Count == 0)
        {
            return string.IsNullOrWhiteSpace(search);
        }

        var s = search.Trim();

        return extra.Values.Any(v =>
            v.ValueKind == JsonValueKind.String &&
            (v.GetString() ?? string.Empty).Contains(s, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>Error factory for the additive admin modules.</summary>
public static class AdminErrors
{
    public static Error NotFound(string resource, Guid id) => Error.NotFound(
        "admin.not_found", $"{resource} '{id}' could not be found.");

    public static Error InvalidStatus(string status) => Error.Validation(
        "admin.invalid_status", $"Status '{status}' is not valid for this resource.");
}
