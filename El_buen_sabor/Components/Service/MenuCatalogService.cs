using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Service
{
    public class MenuCatalogService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public MenuCatalogService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<List<DishDto>> GetDishesAsync()
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/Dishes");
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener platos. Código: {response.StatusCode}");
                    return [];
                }

                return await response.Content.ReadFromJsonAsync<List<DishDto>>() ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener platos: {ex.Message}");
                return [];
            }
        }

        public async Task<bool> CreateDishAsync(CreateDishDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/Dishes");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear plato: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateDishAsync(UpdateDishDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/Dishes/{dto.Id}");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar plato: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDishAsync(Guid id)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/Dishes/{id}");
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar plato: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/Categories");
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener categorías. Código: {response.StatusCode}");
                    return [];
                }

                return await response.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener categorías: {ex.Message}");
                return [];
            }
        }

        public async Task<List<DrinkDto>> GetDrinksAsync()
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/Drinks");
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener bebidas. Código: {response.StatusCode}");
                    return [];
                }

                return await response.Content.ReadFromJsonAsync<List<DrinkDto>>() ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener bebidas: {ex.Message}");
                return [];
            }
        }

        public async Task<bool> CreateDrinkAsync(CreateDrinkDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/Drinks");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear bebida: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateDrinkAsync(Guid id, UpdateDrinkDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/Drinks/{id}");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar bebida: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDrinkAsync(Guid id)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/Drinks/{id}");
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar bebida: {ex.Message}");
                return false;
            }
        }

        public string BuildImageUrl(string? imageUrl)
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

        private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            var token = await _localStorage.GetItemAsync<string>("token");

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return request;
        }
    }
}
