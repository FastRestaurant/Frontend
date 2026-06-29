using System;

namespace El_buen_sabor.Components.Models
{
    public class DishDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool Available { get; set; }
        public int EstimatedPreparationMinutes { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CreateDishDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool Available { get; set; }
        public int EstimatedPreparationMinutes { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateDishDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool Available { get; set; }
        public int EstimatedPreparationMinutes { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CategoryUsageDto
    {
        public Guid CategoryId { get; set; }
        public int DishCount { get; set; }
        public int DrinkCount { get; set; }
        public bool HasProducts => DishCount > 0 || DrinkCount > 0;
    }
}
