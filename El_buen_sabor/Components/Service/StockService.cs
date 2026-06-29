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


        public async Task<PagedResponseDto<IngredientDTO>> GetIngredientsAsync(int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                var query = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Add($"search={Uri.EscapeDataString(search.Trim())}");
                }

                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/v1/ingredients?{string.Join("&", query)}");
                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener los ingredientes. Código: {response.StatusCode}");
                    return new PagedResponseDto<IngredientDTO>();
                }
                return await response.Content.ReadFromJsonAsync<PagedResponseDto<IngredientDTO>>() ?? new PagedResponseDto<IngredientDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener los ingredientes: {ex.Message}");
                return new PagedResponseDto<IngredientDTO>();
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

        public async Task<List<IngredientDTO>> GetAllIngredientsForRecipeAsync()
        {
            var page = await GetIngredientsAsync(1, 1000);
            return page.Items;
        }

        public async Task<List<IngredientDishDto>> GetDishRecipeAsync(Guid dishId)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/v1/ingredient-dishes/dish/{dishId}");
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return [];

                return await response.Content.ReadFromJsonAsync<List<IngredientDishDto>>() ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener receta del plato: {ex.Message}");
                return [];
            }
        }

        public async Task<bool> ReplaceDishRecipeAsync(Guid dishId, List<DishIngredientRequestDto> items)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/v1/ingredient-dishes/dish/{dishId}");
                request.Content = JsonContent.Create(new ReplaceDishIngredientsRequestDto { Items = items });
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar receta del plato: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStockAsync(Guid id, IngredientDTO dto, decimal newStock)
        {
            try
            {
                var rowVersion = await GetStockRowVersionAsync(dto.StockId);
                if (string.IsNullOrWhiteSpace(rowVersion))
                    return false;

                using var requestStock = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/v1/stocks/{dto.StockId}");
                requestStock.Content = JsonContent.Create(new { Count = newStock, RowVersion = rowVersion });
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

        public async Task<DrinkStockDto?> GetDrinkStockAsync(Guid drinkId)
        {
            var stocks = await GetDrinkStocksAsync();
            return stocks.FirstOrDefault(stock => stock.Id_Drink == drinkId);
        }

        public async Task<List<DrinkStockDto>> GetDrinkStocksAsync()
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/v1/stocks");
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return [];

                var stocks = await response.Content.ReadFromJsonAsync<List<DrinkStockDto>>() ?? [];
                return stocks.Where(stock => stock.Id_Drink.HasValue).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener stocks de bebidas: {ex.Message}");
                return [];
            }
        }

        public async Task<bool> CreateDrinkStockAsync(Guid drinkId, decimal count)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/stocks");
                request.Content = JsonContent.Create(new { Count = count, Id_Drink = drinkId });
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear stock de bebida: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateDrinkStockAsync(Guid stockId, decimal newStock)
        {
            try
            {
                var rowVersion = await GetStockRowVersionAsync(stockId);
                if (string.IsNullOrWhiteSpace(rowVersion))
                    return false;

                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/v1/stocks/{stockId}");
                request.Content = JsonContent.Create(new { Count = newStock, RowVersion = rowVersion });
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar stock de bebida: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDrinkStockAsync(Guid stockId)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/v1/stocks/{stockId}");
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar stock de bebida: {ex.Message}");
                return false;
            }
        }

        private async Task<string?> GetStockRowVersionAsync(Guid stockId)
        {
            using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/v1/stocks/{stockId}");
            using var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            var stock = await response.Content.ReadFromJsonAsync<StockResponseDto>();
            return stock?.RowVersion;
        }

        private sealed class StockResponseDto
        {
            public string RowVersion { get; set; } = string.Empty;
        }
    }
}
