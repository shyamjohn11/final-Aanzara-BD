using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IOrderRepository
{
    /// <summary>Full order aggregate: user, items with products, coupon, payments, status history.</summary>
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>The caller's orders, newest first, with items and products loaded.</summary>
    Task<(IReadOnlyList<Order> Orders, int TotalCount)> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Paged, filtered order list for the admin console. Search matches
    /// customer name, email, or the order id.</summary>
    Task<(IReadOnlyList<Order> Orders, int TotalCount)> SearchAsync(
        string? search, OrderStatus? status, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>The most recent orders across all users, newest first.</summary>
    Task<IReadOnlyList<Order>> GetRecentAsync(int count, CancellationToken cancellationToken);

    /// <summary>Orders created within [from, to], newest first — feeds the admin
    /// dashboard and report aggregates. Includes user and items with products.</summary>
    Task<IReadOnlyList<Order>> GetInRangeAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    void Add(Order order);

    void Remove(Order order);

    /// <summary>Stages a payment row for the next SaveChangesAsync (checkout).</summary>
    void AddPayment(Payment payment);

    /// <summary>Stages a status-history row for the next SaveChangesAsync.</summary>
    void AddStatusHistory(OrderStatusHistory entry);

    /// <summary>Stages a shipping-address snapshot for the next SaveChangesAsync.</summary>
    void AddAddress(OrderAddress address);
}
