namespace El_buen_sabor.Components.Models
{
    public class CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
