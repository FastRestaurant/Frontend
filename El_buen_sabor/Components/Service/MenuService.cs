using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Service
{
    public class MenuService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public MenuService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<List<Category>> GetCategories()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "api/Categories");
                using var response = await SendAuthorizedAsync(request);

                if (!response.IsSuccessStatusCode)
                    return [];

                return await response.Content.ReadFromJsonAsync<List<Category>>() ?? [];
            }
            catch
            {
                return [];
            }
        }

        public async Task<PagedResultDto<Product>> GetProductsByCategory(
            Guid categoryId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var dishes = await GetMenuItemsByCategory(
                categoryId,
                "api/Dishes/category",
                ProductTypes.Dish,
                pageNumber,
                pageSize);

            var drinks = await GetMenuItemsByCategory(
                categoryId,
                "api/Drinks/category",
                ProductTypes.Drink,
                pageNumber,
                pageSize);

            var products = dishes.Items
                .Concat(drinks.Items)
                .Where(product => product.Available)
                .OrderBy(product => product.Name)
                .ToList();

            return new PagedResultDto<Product>
            {
                Items = products,
                PageNumber = Math.Max(dishes.PageNumber, drinks.PageNumber),
                PageSize = pageSize,
                TotalItems = dishes.TotalItems + drinks.TotalItems,
                TotalPages = Math.Max(dishes.TotalPages, drinks.TotalPages),
                HasPreviousPage = dishes.HasPreviousPage || drinks.HasPreviousPage,
                HasNextPage = dishes.HasNextPage || drinks.HasNextPage
            };
        }

        private async Task<PagedResultDto<Product>> GetMenuItemsByCategory(
            Guid categoryId,
            string resource,
            string productType,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{resource}/{categoryId}?pageNumber={pageNumber}&pageSize={pageSize}");

                using var response = await SendAuthorizedAsync(request);

                if (!response.IsSuccessStatusCode)
                    return new PagedResultDto<Product>();

                var result = await response.Content.ReadFromJsonAsync<PagedResultDto<MenuItemDto>>();

                if (result is null)
                    return new PagedResultDto<Product>();

                return new PagedResultDto<Product>
                {
                    Items = result.Items.Select(item => new Product
                    {
                        Id = item.Id,
                        CategoryId = item.CategoryId,
                        Name = item.Name,
                        Price = item.Price,
                        Url = item.ImageUrl?.Trim() ?? string.Empty,
                        Available = item.Available,
                        ProductType = productType
                    }).ToList(),

                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
            }
            catch
            {
                return new PagedResultDto<Product>();
            }
        }

        private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpRequestMessage request)
        {
            var token = await _localStorage.GetItemAsync<string>("token");

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _http.SendAsync(request);
        }

        private sealed class MenuItemDto
        {
            public Guid Id { get; set; }
            public Guid CategoryId { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool Available { get; set; }
            public string? ImageUrl { get; set; }
        }
    }
}
