using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class AddressRepository : IAddressRepository
{
    private readonly ApplicationDbContext _db;

    public AddressRepository(ApplicationDbContext db) => _db = db;

    public Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => ListAsync(_db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt), cancellationToken);

    public Task<Address?> GetByIdAndUserIdAsync(Guid addressId, Guid userId, CancellationToken cancellationToken)
        => _db.Addresses.FirstOrDefaultAsync(
            a => a.AddressId == addressId && a.UserId == userId, cancellationToken);

    public void Add(Address address) => _db.Addresses.Add(address);

    public void Remove(Address address) => _db.Addresses.Remove(address);

    private static async Task<IReadOnlyList<Address>> ListAsync(
        IQueryable<Address> query, CancellationToken cancellationToken)
        => await query.ToListAsync(cancellationToken);
}
