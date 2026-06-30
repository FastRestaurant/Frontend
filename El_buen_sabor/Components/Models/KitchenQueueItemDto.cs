namespace El_buen_sabor.Components.Models
{
    public class KitchenQueueItemDto
    {
        public Guid ItemId { get; set; }
        public Guid OrderId { get; set; }
        public int TableNumber { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int EstimatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
