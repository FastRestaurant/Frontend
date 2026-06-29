namespace El_buen_sabor.Components.Models
{
    public class OrderRealtimeEventDto
    {
        public Guid Id { get; set; }
        public Guid TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public Guid WaiterId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderRealtimeItemDto> Items { get; set; } = [];
    }

    public class OrderRealtimeItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public string ProductNameSnapshot { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ReadyAt { get; set; }
    }
}
