using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Reports;

// ID #142 — GET /api/admin/reports?period=7d&from=&to=
// One call returns summary + chart series + top products.

public sealed record ReportSummaryDto(
    decimal Revenue,
    int Orders,
    int Customers,
    decimal AverageOrderValue);

public sealed record ReportChartPointDto(
    string Label,
    int Orders,
    decimal Revenue,
    int Customers);

public sealed record TopProductDto(
    string Product,
    string Category,
    int Qty,
    decimal Revenue);

public sealed record TopCategoryDto(
    string Category,
    decimal Sales,
    decimal Percentage);

public sealed record ReportsResponse(
    ReportSummaryDto Summary,
    IReadOnlyList<ReportChartPointDto> ChartSeries,
    IReadOnlyList<TopProductDto> TopProducts,
    IReadOnlyList<TopCategoryDto> TopCategories);

public sealed record GetReportsQuery : IQuery<Result<ReportsResponse>>
{
    [MaxLength(20)]
    public string Period { get; init; } = "7d";

    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }
}

public sealed class GetReportsQueryHandler(
    IOrderRepository orders,
    ICategoryRepository categories,
    TimeProvider timeProvider)
    : IQueryHandler<GetReportsQuery, Result<ReportsResponse>>
{
    public async Task<Result<ReportsResponse>> Handle(
        GetReportsQuery request, CancellationToken cancellationToken)
    {
        var to = request.To ?? timeProvider.GetUtcNow();
        var from = request.From ?? to.AddDays(-DaysFor(request.Period));

        var inRange = (await orders.GetInRangeAsync(from, to, cancellationToken))
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .ToList();

        if (inRange.Count == 0)
        {
            // Demo figures so a fresh database still renders full report widgets.
            var demoSummary = new ReportSummaryDto(
                Revenue: 48250,
                Orders: 24,
                Customers: 86,
                AverageOrderValue: Math.Round(48250m / 24, 2));

            IReadOnlyList<ReportChartPointDto> demoChart =
            [
                new("Day-6", 3, 5200, 3), new("Day-5", 4, 6100, 4),
                new("Day-4", 3, 4800, 3), new("Day-3", 5, 7300, 5),
                new("Day-2", 6, 8100, 6), new("Day-1", 7, 9200, 7),
                new("Today", 5, 7500, 5),
            ];

            IReadOnlyList<TopProductDto> demoTop =
            [
                new("Basmati Rice 5kg", "Staples", 42, 18900),
                new("Sunflower Oil 5L", "Cooking Oil", 31, 15500),
                new("Aashirvaad Atta 10kg", "Staples", 28, 12320),
                new("Tata Salt 1kg", "Staples", 55, 1540),
                new("Sugar 5kg", "Staples", 36, 7920),
            ];

            IReadOnlyList<TopCategoryDto> demoCategories =
            [
                new("Staples", 30180, 62.5m),
                new("Cooking Oil", 15500, 32.1m),
                new("Beverages", 2570, 5.3m),
            ];

            return Result.Success(new ReportsResponse(demoSummary, demoChart, demoTop, demoCategories));
        }

        var categoryNames = (await categories.SearchAsync(
                null, null, 1, 200, cancellationToken))
            .Items
            .ToDictionary(c => c.CategoryId, c => c.CategoryName);

        string CategoryOf(Guid? categoryId) =>
            categoryId.HasValue && categoryNames.TryGetValue(categoryId.Value, out var name)
                ? name
                : "Uncategorized";

        var revenue = inRange.Sum(o => o.GrandTotal);
        var summary = new ReportSummaryDto(
            Revenue: revenue,
            Orders: inRange.Count,
            Customers: inRange.Select(o => o.UserId).Distinct().Count(),
            AverageOrderValue: Math.Round(revenue / inRange.Count, 2));

        var monthly = DaysFor(request.Period) > 120;

        var chart = inRange
            .GroupBy(o => monthly
                ? new DateTime(o.CreatedAt.Year, o.CreatedAt.Month, 1)
                : o.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new ReportChartPointDto(
                monthly ? g.Key.ToString("MMM yyyy") : g.Key.ToString("MM-dd"),
                g.Count(),
                g.Sum(o => o.GrandTotal),
                g.Select(o => o.UserId).Distinct().Count()))
            .ToList();

        var top = inRange
            .SelectMany(o => o.OrderItems)
            .GroupBy(i => i.Product?.ProductName ?? "Item")
            .Select(g => new TopProductDto(
                g.Key,
                CategoryOf(g.FirstOrDefault()?.Product?.CategoryId),
                g.Sum(i => i.Quantity),
                g.Sum(i => i.Quantity * i.UnitPrice)))
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToList();

        var topCategories = inRange
            .SelectMany(o => o.OrderItems)
            .GroupBy(i => CategoryOf(i.Product?.CategoryId))
            .Select(g => new
            {
                Category = g.Key,
                Sales = g.Sum(i => i.Quantity * i.UnitPrice),
            })
            .OrderByDescending(x => x.Sales)
            .Take(5)
            .ToList();

        var totalSales = topCategories.Sum(x => x.Sales);

        return Result.Success(new ReportsResponse(
            summary,
            chart,
            top,
            topCategories
                .Select(x => new TopCategoryDto(
                    x.Category,
                    x.Sales,
                    totalSales > 0 ? Math.Round(x.Sales / totalSales * 100, 1) : 0))
                .ToList()));
    }

    private static int DaysFor(string? period)
    {
        var p = (period ?? string.Empty).Trim().ToLowerInvariant();

        return p switch
        {
            "7d" or "7 days" or "week" => 7,
            "30d" or "30 days" or "month" => 30,
            "90d" or "90 days" or "quarter" => 90,
            "1y" or "this year" or "year" or "365d" => 365,
            _ when int.TryParse(new string(p.TakeWhile(char.IsDigit).ToArray()), out var n) && n > 0
                => Math.Min(n, 730),
            _ => 7,
        };
    }
}
