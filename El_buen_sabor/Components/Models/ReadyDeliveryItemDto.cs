namespace El_buen_sabor.Components.Models
{
    public class ReadyDeliveryItemDto
    {
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
        public Guid TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime ReadyAt { get; set; }
        public bool WasDelayed { get; set; }
    }
}
