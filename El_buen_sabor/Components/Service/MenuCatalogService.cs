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

        // =========================
        // DISHES - PLATOS
        // =========================

        public async Task<PagedResultDto<DishDto>> GetDishesAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await GetAsync<PagedResultDto<DishDto>>(
                $"api/Dishes?pageNumber={pageNumber}&pageSize={pageSize}",
                new PagedResultDto<DishDto>(),
                "obtener platos"
            );
        }

        public async Task<bool> CreateDishAsync(CreateDishDto dto)
        {
            return await SendWithBodyAsync(HttpMethod.Post, "api/Dishes", dto, "crear plato");
        }

        public async Task<bool> UpdateDishAsync(UpdateDishDto dto)
        {
            return await SendWithBodyAsync(HttpMethod.Put, $"api/Dishes/{dto.Id}", dto, "actualizar plato");
        }

        public async Task<bool> DeleteDishAsync(Guid id)
        {
            return await SendAsync(HttpMethod.Delete, $"api/Dishes/{id}", "eliminar plato");
        }

        // =========================
        // DRINKS - BEBIDAS
        // =========================

        public async Task<PagedResultDto<DrinkDto>> GetDrinksAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await GetAsync<PagedResultDto<DrinkDto>>(
                $"api/Drinks?pageNumber={pageNumber}&pageSize={pageSize}",
                new PagedResultDto<DrinkDto>(),
                "obtener bebidas"
            );
        }

        public async Task<bool> CreateDrinkAsync(CreateDrinkDto dto)
        {
            return await SendWithBodyAsync(HttpMethod.Post, "api/Drinks", dto, "crear bebida");
        }

        public async Task<bool> UpdateDrinkAsync(Guid id, UpdateDrinkDto dto)
        {
            return await SendWithBodyAsync(HttpMethod.Put, $"api/Drinks/{id}", dto, "actualizar bebida");
        }

        public async Task<bool> DeleteDrinkAsync(Guid id)
        {
            return await SendAsync(HttpMethod.Delete, $"api/Drinks/{id}", "eliminar bebida");
        }

        // =========================
        // CATEGORIES - CATEGORÍAS
        // =========================

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            return await GetAsync<List<CategoryDto>>(
                "api/Categories",
                [],
                "obtener categorías"
            );
        }

        // =========================
        // HELPERS HTTP
        // =========================

        private async Task<T> GetAsync<T>(string url, T defaultValue, string operationName)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, url);
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al {operationName}. Código: {response.StatusCode}");
                    return defaultValue;
                }

                return await response.Content.ReadFromJsonAsync<T>() ?? defaultValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al {operationName}: {ex.Message}");
                return defaultValue;
            }
        }

        private async Task<bool> SendAsync(HttpMethod method, string url, string operationName)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(method, url);
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al {operationName}. Código: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al {operationName}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendWithBodyAsync<T>(HttpMethod method, string url, T dto, string operationName)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(method, url);
                request.Content = JsonContent.Create(dto);

                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al {operationName}. Código: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al {operationName}: {ex.Message}");
                return false;
            }
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