using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IAddressRepository
{
    /// <summary>The caller's saved addresses, default first.</summary>
    Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Scoped by userId so one caller can never touch another's address by guessing an id.</summary>
    Task<Address?> GetByIdAndUserIdAsync(Guid addressId, Guid userId, CancellationToken cancellationToken);

    void Add(Address address);

    void Remove(Address address);
}
