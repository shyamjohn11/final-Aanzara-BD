using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses;

public static class WarehouseErrors
{
    public static readonly Error WarehouseNotFound =
        Error.NotFound("warehouse.not_found", "The warehouse could not be found.");

    public static Error WarehouseNameTaken(string name) => Error.Conflict(
        "warehouse.name_taken", $"Warehouse name '{name}' is already in use.");

    public static readonly Error WarehouseHasInventory = Error.Conflict(
        "warehouse.has_inventory",
        "This warehouse still has inventory assigned to it. Clear or reassign inventory first.");
}
