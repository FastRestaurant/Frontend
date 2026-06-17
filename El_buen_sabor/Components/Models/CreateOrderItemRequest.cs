namespace El_buen_sabor.Components.Models
{
    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
    }
}
