namespace El_buen_sabor.Components.Models
{
    public class CreateOrderRequest
    {
        public int TableId { get; set; }

        public List<CreateOrderItemRequest> Items { get; set; } = [];




    }
}
