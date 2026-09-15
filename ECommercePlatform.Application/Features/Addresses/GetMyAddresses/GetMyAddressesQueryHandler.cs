using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Addresses.GetMyAddresses;

public sealed class GetMyAddressesQueryHandler(IAddressRepository addresses)
    : IQueryHandler<GetMyAddressesQuery, Result<IReadOnlyList<AddressResponse>>>
{
    public async Task<Result<IReadOnlyList<AddressResponse>>> Handle(
        GetMyAddressesQuery request, CancellationToken cancellationToken)
    {
        var found = await addresses.GetByUserIdAsync(request.UserId, cancellationToken);

        return Result.Success<IReadOnlyList<AddressResponse>>(
            found.Select(AddressResponse.From).ToList());
    }
}
