namespace El_buen_sabor.Components.Models
{
    public class IngredientDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid StockId { get; set; }
        public decimal StockCount { get; set; }
        public int UnitType { get; set; }
    }
    public class CreateIngredientDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal InitialStock { get; set; }
        public int UnitType { get; set; }
    }

    public class UpdateIngredientDto
    {

        public string Name { get; set; } = string.Empty;
        public int UnitType { get; set; }

    }
    public class DeleteIngredientDto
    {
        public Guid Id { get; set; }
    }
    public class IngredientStockUpdateDto
    {
        public decimal StockCount { get; set; }
    }

    public class IngredientDishDto
    {
        public Guid IdIngredientDish { get; set; }
        public Guid Id_Ingredient { get; set; }
        public Guid Id_Dish { get; set; }
        public decimal RequiredQuantity { get; set; }
    }

    public class DishIngredientRequestDto
    {
        public Guid Id_Ingredient { get; set; }
        public decimal RequiredQuantity { get; set; }
    }

    public class ReplaceDishIngredientsRequestDto
    {
        public List<DishIngredientRequestDto> Items { get; set; } = new();
    }
}
