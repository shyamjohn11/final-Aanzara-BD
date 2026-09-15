using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory;

public static class InventoryErrors
{
    public static readonly Error InventoryNotFound =
        Error.NotFound("inventory.not_found", "No inventory record exists for the specified product and warehouse.");

    public static readonly Error InvalidQuantity =
        Error.Validation("inventory.invalid_quantity", "Quantity must be greater than zero for receiving stock.");

    public static readonly Error StockCannotBeNegative =
        Error.Conflict("inventory.stock_cannot_be_negative", "Stock quantity cannot drop below zero.");

    public static readonly Error InvalidMovementType =
        Error.Validation("inventory.invalid_movement_type", "Invalid inventory movement type.");
}
