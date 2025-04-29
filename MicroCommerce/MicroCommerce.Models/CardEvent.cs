namespace MicroCommerce.Models
{
    public class CardEvent: EventBase
    {
        public CardEventTypeEnum Type { get; set; }
        public Order Order { get; set; }
    }
}
