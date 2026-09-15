using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Addresses.DeleteAddress;

public sealed class DeleteAddressCommandHandler(
    IAddressRepository addresses,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAddressCommand, Result>
{
    public async Task<Result> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await addresses.GetByIdAndUserIdAsync(request.AddressId, request.UserId, cancellationToken);
        if (address is null)
        {
            return Result.Failure(AddressErrors.NotFound);
        }

        addresses.Remove(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
