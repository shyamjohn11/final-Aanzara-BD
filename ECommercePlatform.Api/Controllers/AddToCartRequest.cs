namespace ECommercePlatform.Api.Controllers;

public sealed record AddToCartRequest(Guid ProductId, int Quantity);