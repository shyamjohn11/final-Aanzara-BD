using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IDeliveryRuleRepository
{
    /// <summary>
    /// The active delivery-charge configuration. Null if none has been set up
    /// yet, in which case callers should charge nothing rather than guess.
    /// </summary>
    Task<DeliveryRule?> GetAsync(CancellationToken cancellationToken);
}