using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Addresses;

public static class AddressErrors
{
    public static readonly Error NotAuthenticated =
        Error.Unauthorized("addresses.not_authenticated", "You must be signed in to manage addresses.");

    public static readonly Error NotFound =
        Error.NotFound("addresses.not_found", "Address not found.");

    public static readonly Error MissingFields =
        Error.Validation(
            "addresses.missing_fields",
            "Address line 1, city, state, and pincode are required.");
}
