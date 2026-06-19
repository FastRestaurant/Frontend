using El_buen_sabor.Components.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace El_buen_sabor.Components.Service
{
    public class MenuCatalogService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public MenuCatalogService(ILocalStorageService localStorage, string baseUrl)
        {
            _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _localStorage = localStorage;
        }

        private async Task<HttpRequestMessage> CrearRequestConToken(HttpMethod metodo, string url)
        {
            var request = new HttpRequestMessage(metodo, url);
            var token = await _localStorage.GetItemAsync<string>("token");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return request;
        }

        public async Task<List<DishDto>> GetDishesAsync()
        {
            try
            {
                string url = $"{_http.BaseAddress}api/Dishes";
                using var request = await CrearRequestConToken(HttpMethod.Get, url);
                using var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var dishes = await response.Content.ReadFromJsonAsync<List<DishDto>>();
                    return dishes ?? new List<DishDto>();
                }
                else
                {
                    Console.WriteLine($"Error al obtener platos. Código: {response.StatusCode}");
                    return new List<DishDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener platos: {ex.Message}");
                return new List<DishDto>();
            }
        }

        public async Task<bool> CreateDishAsync(CreateDishDto dto)
        {
            try
            {
                string url = $"{_http.BaseAddress}api/Dishes";
                using var request = await CrearRequestConToken(HttpMethod.Post, url);
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
                string url = $"{_http.BaseAddress}api/Dishes/{dto.Id}";
                using var request = await CrearRequestConToken(HttpMethod.Put, url);
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
                string url = $"{_http.BaseAddress}api/Dishes/{id}";
                using var request = await CrearRequestConToken(HttpMethod.Delete, url);
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
                string url = $"{_http.BaseAddress}api/Categories";
                var token = await _localStorage.GetItemAsync<string>("token");
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var cats = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
                    return cats ?? new List<CategoryDto>();
                }
                return new List<CategoryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener categorías: {ex.Message}");
                return new List<CategoryDto>();
            }
        }

        public async Task<List<DrinkDto>> GetDrinksAsync()
        {
            try
            {
                string url = $"{_http.BaseAddress}api/Drinks";
                var token = await _localStorage.GetItemAsync<string>("token");
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var drinks = await response.Content.ReadFromJsonAsync<List<DrinkDto>>();
                    return drinks ?? new List<DrinkDto>();
                }
                Console.WriteLine($"Error al obtener bebidas. Código: {response.StatusCode}");
                return new List<DrinkDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener bebidas: {ex.Message}");
                return new List<DrinkDto>();
            }
        }

        public async Task<bool> CreateDrinkAsync(CreateDrinkDto dto)
        {
            try
            {
                string url = $"{_http.BaseAddress}api/Drinks";
                var token = await _localStorage.GetItemAsync<string>("token");
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
                string url = $"{_http.BaseAddress}api/Drinks/{id}";
                var token = await _localStorage.GetItemAsync<string>("token");
                using var request = new HttpRequestMessage(HttpMethod.Put, url);
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
                string url = $"{_http.BaseAddress}api/Drinks/{id}";
                var token = await _localStorage.GetItemAsync<string>("token");
                using var request = new HttpRequestMessage(HttpMethod.Delete, url);
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar bebida: {ex.Message}");
                return false;
            }
        }
    }
}