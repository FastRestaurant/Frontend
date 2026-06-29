using System;

namespace El_buen_sabor.Components.Models
{
    public class DrinkDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool Available { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? StockId { get; set; }
        public decimal StockCount { get; set; }
    }

    public class CreateDrinkDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool Available { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateDrinkDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool Available { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class DrinkStockDto
    {
        public Guid Id { get; set; }
        public decimal Count { get; set; }
        public string RowVersion { get; set; } = string.Empty;
        public Guid? Id_Drink { get; set; }
    }
}
