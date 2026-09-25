using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;

    public OrderRepository(ApplicationDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
        => _db.Orders
            // Split avoids a wide cartesian product when OrderItems × Payments
            // × StatusHistory all fan out on the same root row.
            .AsSplitQuery()
            .Include(o => o.User)
            .Include(o => o.AppliedCoupon)
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .Include(o => o.StatusHistory)
            .Include(o => o.ShippingAddress)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

    public async Task<(IReadOnlyList<Order> Orders, int TotalCount)> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .Include(o => o.StatusHistory)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync(cancellationToken);

        var orders = await query
            .Skip((Math.Max(1, page) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (orders, total);
    }

    public async Task<(IReadOnlyList<Order> Orders, int TotalCount)> SearchAsync(
        string? search, OrderStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .AsQueryable();

        if (status is { } statusValue)
        {
            query = query.Where(o => o.OrderStatus == statusValue);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            // The schema has no order-number column, so callers search by the
            // customer identity or by the exact order id.
            query = query.Where(o =>
                o.User.Name.Contains(term) ||
                o.User.Email.Contains(term) ||
                o.OrderId.ToString().Contains(term));
        }

        var ordered = query.OrderByDescending(o => o.CreatedAt);

        var total = await ordered.CountAsync(cancellationToken);

        var orders = await ordered
            .Skip((Math.Max(1, page) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (orders, total);
    }

    public async Task<IReadOnlyList<Order>> GetRecentAsync(int count, CancellationToken cancellationToken)
        => await _db.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> GetInRangeAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
        => await _db.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Add(Order order) => _db.Orders.Add(order);

    public void Remove(Order order) => _db.Orders.Remove(order);

    public void AddPayment(Payment payment) => _db.Payments.Add(payment);

    public void AddStatusHistory(OrderStatusHistory entry) => _db.OrderStatusHistory.Add(entry);

    public void AddAddress(OrderAddress address) => _db.OrderAddresses.Add(address);
}
