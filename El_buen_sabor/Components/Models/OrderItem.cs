namespace El_buen_sabor.Components.Models
{
    public class OrderFromTable
    {
        public Guid Id { get; set; }

        public Product Producto { get; set; } = null!;

        public int Cantidad { get; set; }

        public decimal Subtotal =>
            Producto.Price * Cantidad;
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ReadyAt { get; set; }
    }
}
