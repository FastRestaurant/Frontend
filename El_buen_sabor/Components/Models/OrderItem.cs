namespace El_buen_sabor.Components.Models
{
    public class OrderFromTable
    {
        public Product Producto { get; set; } = null!;

        public int Cantidad { get; set; }

        public decimal Subtotal =>
            Producto.Price * Cantidad;
        public string Notes { get; set; }
    }
}
