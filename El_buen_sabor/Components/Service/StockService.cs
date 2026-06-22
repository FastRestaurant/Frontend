using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using El_buen_sabor.Components.Interface;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Service
{
    public class StockService
    {

        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public StockService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }


        public async Task<List<IngredientDTO>> GetIngredientsAsync()
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/v1/ingredients");
                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener los ingredientes. Código: {response.StatusCode}");
                    return [];
                }
                return await response.Content.ReadFromJsonAsync<List<IngredientDTO>>() ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener los ingredientes: {ex.Message}");
                return [];
            }
        }

        public async Task<bool> CreateIngredientAsync(CreateIngredientDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/ingredients");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear ingrediente: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateIngredientAsync(Guid id, UpdateIngredientDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/v1/ingredients/{id}");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar ingrediente: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteIngredientAsync(Guid id)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/v1/ingredients/{id}");
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar ingrediente: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStockAsync(Guid id, IngredientDTO dto, int newStock)
        {
            try
            {
                using var requestIngredient = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/v1/ingredients/{id}");
                requestIngredient.Content = JsonContent.Create(dto);
                using var requestStock = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/v1/stocks/{dto.StockId}");
                requestStock.Content = JsonContent.Create(new { Count = newStock });
                using var response = await _http.SendAsync(requestStock);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar stock: {ex.Message}");
                return false;
            }
        }

        //public string BuildImageUrl(string? imageUrl)
        //{
        //    if (string.IsNullOrWhiteSpace(imageUrl))
        //        return string.Empty;

        //    var trimmedUrl = imageUrl.Trim();
        //    if (Uri.TryCreate(trimmedUrl, UriKind.Absolute, out _))
        //        return trimmedUrl;

        //    return _http.BaseAddress is null
        //        ? trimmedUrl
        //        : new Uri(_http.BaseAddress, trimmedUrl).ToString();
        //}

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
