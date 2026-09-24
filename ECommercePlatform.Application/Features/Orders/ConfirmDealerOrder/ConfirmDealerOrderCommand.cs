using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Orders.ConfirmDealerOrder;

public sealed record ConfirmDealerOrderCommand(Guid OrderId, Guid DealerId, Guid AgentId) : ICommand<Result>;
