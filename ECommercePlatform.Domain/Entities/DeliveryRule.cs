namespace ECommercePlatform.Domain.Entities
{
    public class DeliveryRule
    {
        public Guid DeliveryRuleId { get; set; }

        public decimal MinOrderValueForFreeDelivery { get; set; }

        public decimal FlatDeliveryCharge { get; set; }

        public decimal HandlingFee { get; set; }
    }
}