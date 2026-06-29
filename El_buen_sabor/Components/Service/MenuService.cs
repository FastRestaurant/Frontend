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
                using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/categories");
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

        public async Task<List<Product>> GetProductsByCategory(Guid categoryId)
        {
            var dishes = await GetMenuItemsByCategory(categoryId, "api/v1/dishes/category", ProductTypes.Dish);
            var drinks = await GetMenuItemsByCategory(categoryId, "api/v1/drinks/category", ProductTypes.Drink);

            return dishes
                .Concat(drinks)
                .Where(product => product.Available)
                .OrderBy(product => product.Name)
                .ToList();
        }

        private async Task<List<Product>> GetMenuItemsByCategory(Guid categoryId, string resource, string productType)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{resource}/{categoryId}");
                using var response = await SendAuthorizedAsync(request);

                if (!response.IsSuccessStatusCode)
                    return [];

                var result = await response.Content.ReadFromJsonAsync<PagedResponseDto<MenuItemDto>>();
                var items = result?.Items ?? [];

                return items.Select(item => new Product
                {
                    Id = item.Id,
                    CategoryId = item.CategoryId,
                    Name = item.Name,
                    Price = item.Price,
                    Url = BuildImageUrl(item.ImageUrl),
                    Available = item.Available,
                    ProductType = productType
                })
                .ToList();
            }
            catch
            {
                return [];
            }
        }

        private string BuildImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return string.Empty;

            var trimmedUrl = imageUrl.Trim();
            if (Uri.TryCreate(trimmedUrl, UriKind.Absolute, out _))
                return trimmedUrl;

            return _http.BaseAddress is null
                ? trimmedUrl
                : new Uri(_http.BaseAddress, trimmedUrl).ToString();
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
