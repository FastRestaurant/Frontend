namespace El_buen_sabor.Components.Models
{
    public class TableOrdersSummaryDto
    {
        public Guid TableId { get; set; }
        public decimal Total { get; set; }
        public bool CanClose { get; set; }
        public List<Order> Orders { get; set; } = [];
    }
}
