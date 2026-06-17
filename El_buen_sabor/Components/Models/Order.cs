namespace El_buen_sabor.Components.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public List<OrderFromTable> OrderItems { get; set; } = [];
    }
}
