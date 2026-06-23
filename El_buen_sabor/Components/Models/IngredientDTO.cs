namespace El_buen_sabor.Components.Models
{
    public class IngredientDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid StockId { get; set; }
        public int StockCount { get; set; }
    }
    public class CreateIngredientDto
    {
        public string Name { get; set; }
        public int InitialStock { get; set; }
    }

    public class UpdateIngredientDto
    {

        public string Name { get; set; }

    }
    public class DeleteIngredientDto
    {
        public Guid Id { get; set; }
    }
    public class IngredientStockUpdateDto
    {
        public int StockCount { get; set; }
    }
}