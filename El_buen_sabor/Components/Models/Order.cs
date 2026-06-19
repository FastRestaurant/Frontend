namespace El_buen_sabor.Components.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
        public List<OrderFromTable> OrderItems { get; set; } = [];
    }
}
