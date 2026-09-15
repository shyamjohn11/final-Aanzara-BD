using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Addresses.UpdateAddress;

public sealed class UpdateAddressCommandHandler(
    IAddressRepository addresses,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateAddressCommand, Result<AddressResponse>>
{
    public async Task<Result<AddressResponse>> Handle(
        UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await addresses.GetByIdAndUserIdAsync(request.AddressId, request.UserId, cancellationToken);
        if (address is null)
        {
            return Result.Failure<AddressResponse>(AddressErrors.NotFound);
        }

        if (new[] { request.AddressLine1, request.City, request.State, request.Pincode }
                .Any(string.IsNullOrWhiteSpace))
        {
            return Result.Failure<AddressResponse>(AddressErrors.MissingFields);
        }

        var now = timeProvider.GetUtcNow();

        address.Label = string.IsNullOrWhiteSpace(request.Label) ? null : request.Label.Trim();
        address.AddressLine1 = request.AddressLine1.Trim();
        address.AddressLine2 = string.IsNullOrWhiteSpace(request.AddressLine2) ? null : request.AddressLine2.Trim();
        address.City = request.City.Trim();
        address.State = request.State.Trim();
        address.Pincode = request.Pincode.Trim();
        address.Latitude = request.Latitude ?? address.Latitude;
        address.Longitude = request.Longitude ?? address.Longitude;
        address.UpdatedAt = now;

        if (request.IsDefault && !address.IsDefault)
        {
            var existing = await addresses.GetByUserIdAsync(request.UserId, cancellationToken);

            foreach (var other in existing.Where(a => a.IsDefault && a.AddressId != address.AddressId))
            {
                other.IsDefault = false;
                other.UpdatedAt = now;
            }

            address.IsDefault = true;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(AddressResponse.From(address));
    }
}
