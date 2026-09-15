using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Addresses;
using ECommercePlatform.Application.Features.Addresses.CreateAddress;
using ECommercePlatform.Application.Features.Addresses.DeleteAddress;
using ECommercePlatform.Application.Features.Addresses.GetMyAddresses;
using ECommercePlatform.Application.Features.Addresses.UpdateAddress;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// The signed-in caller's saved shipping addresses, used at checkout.
/// </summary>
[Authorize]
[Route("api/v1/addresses")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class AddressesController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(ISender sender, ICurrentUser currentUser, ILogger<AddressesController> logger)
        : base(sender)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>The caller's addresses, default first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AddressResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AddressResponse>>> GetMy(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(AddressErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new GetMyAddressesQuery(userId), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>
    /// Saves a new address. The first address a user saves becomes their default
    /// regardless of the flag, so checkout always has a starting point.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddressResponse>> Create(
        [FromBody] AddressRequest request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(AddressErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new CreateAddressCommand(
            userId,
            request.Label,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.Pincode,
            request.Latitude,
            request.Longitude,
            request.IsDefault), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>Updates one of the caller's addresses in place.</summary>
    [HttpPut("{addressId:guid}")]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressResponse>> Update(
        Guid addressId, [FromBody] AddressRequest request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(AddressErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new UpdateAddressCommand(
            userId,
            addressId,
            request.Label,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.Pincode,
            request.Latitude,
            request.Longitude,
            request.IsDefault), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>Deletes one of the caller's addresses.</summary>
    [HttpDelete("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid addressId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ToProblem(AddressErrors.NotAuthenticated);
        }

        var result = await Sender.Send(new DeleteAddressCommand(userId, addressId), cancellationToken);

        return ToNoContent(result);
    }
}

public sealed record AddressRequest(
    string? Label,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string Pincode,
    decimal? Latitude,
    decimal? Longitude,
    bool IsDefault);
