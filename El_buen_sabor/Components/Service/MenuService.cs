namespace El_buen_sabor.Components.Service
{
    using System.Net.Http.Json;
    using El_buen_sabor.Components.Models;

    public class MenuService
    {
        private readonly HttpClient _http;

        public MenuService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Category>> GetCategories() // TRAER LAS CATEGORIAS EXISTENTES 
        {
            return await _http.GetFromJsonAsync<List<Category>>("api/categories");
        }

        public async Task<List<Product>> GetProductsByCategory(int categoryId) // TRAE PRODUCTOS POR ID DE CATEGORIA
        {
            return await _http.GetFromJsonAsync<List<Product>>(
                $"api/products/category/{categoryId}"
            );
        }


        public Task<List<Category>> GetCategories_test()
        {
            var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Bebidas" },
            new Category { Id = 2, Name = "Entradas" },
            new Category { Id = 3, Name = "Principal" },
            new Category { Id = 4, Name = "Postres" }
        };

            return Task.FromResult(categories);
        }

        // MOCK DE PRODUCTOS POR CATEGORÍA CAMBIAR LUEGO A LOS NO TEST
        public Task<List<Product>> GetProductsByCategory_test(int categoryId)
        {
            var allProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Coca Cola", Price = 1200, Url = "", CategoryId = 1 },
                new Product { Id = 2, Name = "Pepsi", Price = 1200, Url = "", CategoryId = 1 },
                new Product { Id = 3, Name = "Sprite", Price = 1200, Url = "", CategoryId = 1 },
                new Product { Id = 4, Name = "Fanta", Price = 1200, Url = "", CategoryId = 1 },
                new Product { Id = 5, Name = "Agua sin gas", Price = 800, Url = "", CategoryId = 1 },
                new Product { Id = 6, Name = "Agua con gas", Price = 900, Url = "", CategoryId = 1 },
                new Product { Id = 7, Name = "Jugo de naranja", Price = 1500, Url = "", CategoryId = 1 },
                new Product { Id = 8, Name = "Limonada", Price = 1600, Url = "", CategoryId = 1 },
                new Product { Id = 9, Name = "Cerveza", Price = 2500, Url = "", CategoryId = 1 },
                new Product { Id = 10, Name = "Red Bull", Price = 3000, Url = "", CategoryId = 1 },

                new Product { Id = 11, Name = "Empanadas (x6)", Price = 4000, Url = "", CategoryId = 2 },
                new Product { Id = 12, Name = "Rabas", Price = 5500, Url = "", CategoryId = 2 },
                new Product { Id = 13, Name = "Papas bravas", Price = 3000, Url = "", CategoryId = 2 },
                new Product { Id = 14, Name = "Nachos con queso", Price = 4200, Url = "", CategoryId = 2 },
                new Product { Id = 15, Name = "Provoleta", Price = 3800, Url = "", CategoryId = 2 },
                new Product { Id = 16, Name = "Bruschettas", Price = 3500, Url = "", CategoryId = 2 },
                new Product { Id = 17, Name = "Chorizo al pan", Price = 4000, Url = "", CategoryId = 2 },
                new Product { Id = 18, Name = "Morcilla", Price = 3500, Url = "", CategoryId = 2 },
                new Product { Id = 19, Name = "Tabla de fiambres", Price = 7000, Url = "", CategoryId = 2 },
                new Product { Id = 20, Name = "Croquetas de jamón", Price = 4200, Url = "", CategoryId = 2 },

                new Product { Id = 21, Name = "Hamburguesa simple", Price = 5000, Url = "", CategoryId = 3 },
                new Product { Id = 22, Name = "Hamburguesa doble", Price = 6500, Url = "", CategoryId = 3 },
                new Product { Id = 23, Name = "Pizza muzzarella", Price = 7000, Url = "", CategoryId = 3 },
                new Product { Id = 24, Name = "Pizza especial", Price = 8500, Url = "", CategoryId = 3 },
                new Product { Id = 25, Name = "Milanesa napolitana", Price = 7500, Url = "", CategoryId = 3 },
                new Product { Id = 26, Name = "Pasta boloñesa", Price = 6800, Url = "", CategoryId = 3 },
                new Product { Id = 27, Name = "Ravioles", Price = 7200, Url = "", CategoryId = 3 },
                new Product { Id = 28, Name = "Pollo grillado", Price = 7000, Url = "", CategoryId = 3 },
                new Product { Id = 29, Name = "Asado", Price = 9500, Url = "", CategoryId = 3 },
                new Product { Id = 30, Name = "Lomo al plato", Price = 9800, Url = "", CategoryId = 3 },

                new Product { Id = 31, Name = "Flan con dulce de leche", Price = 2500, Url = "", CategoryId = 4 },
                new Product { Id = 32, Name = "Helado 2 bochas", Price = 2200, Url = "", CategoryId = 4 },
                new Product { Id = 33, Name = "Torta de chocolate", Price = 3000, Url = "", CategoryId = 4 },
                new Product { Id = 34, Name = "Cheesecake", Price = 3500, Url = "", CategoryId = 4 },
                new Product { Id = 35, Name = "Brownie", Price = 2800, Url = "", CategoryId = 4 },
                new Product { Id = 36, Name = "Panqueques con dulce", Price = 2700, Url = "", CategoryId = 4 },
                new Product { Id = 37, Name = "Arroz con leche", Price = 2000, Url = "", CategoryId = 4 },
                new Product { Id = 38, Name = "Tiramisú", Price = 3800, Url = "", CategoryId = 4 },
                new Product { Id = 39, Name = "Frutillas con crema", Price = 2600, Url = "", CategoryId = 4 },
                new Product { Id = 40, Name = "Helado artesanal", Price = 3200, Url = "", CategoryId = 4 }
             };

            var result = allProducts
                .Where(p => p.CategoryId == categoryId)
                .ToList();

            return Task.FromResult(result);
        }
    }
}