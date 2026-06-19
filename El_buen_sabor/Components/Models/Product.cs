namespace El_buen_sabor.Components.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Url { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public bool Available { get; set; } = true;

    }
}
