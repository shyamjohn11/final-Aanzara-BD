using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Entities;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Addresses.CreateAddress;

public sealed class CreateAddressCommandHandler(
    IAddressRepository addresses,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateAddressCommand, Result<AddressResponse>>
{
    public async Task<Result<AddressResponse>> Handle(
        CreateAddressCommand request, CancellationToken cancellationToken)
    {
        if (new[] { request.AddressLine1, request.City, request.State, request.Pincode }
                .Any(string.IsNullOrWhiteSpace))
        {
            return Result.Failure<AddressResponse>(AddressErrors.MissingFields);
        }

        var now = timeProvider.GetUtcNow();

        var existing = await addresses.GetByUserIdAsync(request.UserId, cancellationToken);

        // Everyone needs one reachable address; the first one saved is it.
        var isDefault = request.IsDefault || existing.Count == 0;

        var address = new Address
        {
            AddressId = Guid.NewGuid(),
            UserId = request.UserId,
            Label = string.IsNullOrWhiteSpace(request.Label) ? null : request.Label.Trim(),
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = string.IsNullOrWhiteSpace(request.AddressLine2) ? null : request.AddressLine2.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim(),
            Pincode = request.Pincode.Trim(),
            Latitude = request.Latitude ?? 0m,
            Longitude = request.Longitude ?? 0m,
            IsDefault = isDefault,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (isDefault)
        {
            foreach (var other in existing.Where(a => a.IsDefault))
            {
                other.IsDefault = false;
                other.UpdatedAt = now;
            }
        }

        addresses.Add(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(AddressResponse.From(address));
    }
}
