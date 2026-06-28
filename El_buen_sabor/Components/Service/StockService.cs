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
        private readonly HttpClient _menuHttp;
        private readonly ILocalStorageService _localStorage;

        public StockService(
            HttpClient http,
            IHttpClientFactory httpClientFactory,
            ILocalStorageService localStorage)
        {
            _http = http;
            _menuHttp = httpClientFactory.CreateClient("MenuApi");
            _localStorage = localStorage;
        }


        public async Task<PagedResponseDto<IngredientDTO>> GetIngredientsAsync(int page, int pageSize)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(
                    HttpMethod.Get,
                    $"api/v1/ingredients?page={page}&pageSize={pageSize}");
                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener los ingredientes. Código: {response.StatusCode}");
                    return new PagedResponseDto<IngredientDTO>();
                }
                return await response.Content.ReadFromJsonAsync<PagedResponseDto<IngredientDTO>>()
                    ?? new PagedResponseDto<IngredientDTO>();
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
        public async Task<bool> UpdateDrinkStockAsync(Guid stockId, int newStock)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(
                    HttpMethod.Put,
                    $"api/v1/stocks/{stockId}"
                );

                request.Content = JsonContent.Create(new { Count = newStock });

                using var response = await _http.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar stock de bebida: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteIngredientAsync(Guid id, Guid stockId)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/v1/ingredients/{id}");
                using var response = await _http.SendAsync(request);
                using var requeststock = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/v1/stocks/{stockId}");
                using var responsestock = await _http.SendAsync(requeststock);

                return response.IsSuccessStatusCode && responsestock.IsSuccessStatusCode;
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

        public async Task<DrinkDto?> GetDrinkByStockAsync(StockDto stock)
        {
            try
            {
                if (stock.Id_Drink is null)
                    return null;

                using var request = await CreateAuthorizedRequestAsync(
                    HttpMethod.Get,
                    $"api/Drinks/{stock.Id_Drink}"
                );

                using var response = await _menuHttp.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener bebida. Código: {response.StatusCode}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<DrinkDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener bebida: {ex.Message}");
                return null;
            }
        }

        public async Task<PagedResponseDto<DrinkStockDto>> GetDrinkStockAsync(int page, int pageSize)
        {
            try
            {
                using var requestStock = await CreateAuthorizedRequestAsync(
                    HttpMethod.Get,
                    $"api/v1/stocks?page={page}&pageSize={pageSize}&onlyDrinks=true"
                );

                using var responseStock = await _http.SendAsync(requestStock);

                if (!responseStock.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener stock. Código: {responseStock.StatusCode}");
                    return new PagedResponseDto<DrinkStockDto>();
                }

                var stockPage = await responseStock.Content.ReadFromJsonAsync<PagedResponseDto<StockDto>>()
                    ?? new PagedResponseDto<StockDto>();

                var drinkStocks = new List<DrinkStockDto>();

                foreach (var stock in stockPage.Items)
                {
                    if (stock.Id_Drink is null)
                        continue;

                    var drink = await GetDrinkByStockAsync(stock);

                    if (drink is null)
                        continue;

                    drinkStocks.Add(new DrinkStockDto
                    {
                        StockId = stock.Id,
                        Name = drink.Name,
                        Count = stock.Count
                    });
                }
                
                return new PagedResponseDto<DrinkStockDto>
                {
                    Items = drinkStocks,
                    Page = stockPage.Page,
                    PageSize = stockPage.PageSize,
                    TotalItems = stockPage.TotalItems,
                    TotalPages = stockPage.TotalPages
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener stock de bebidas: {ex.Message}");
                return new PagedResponseDto<DrinkStockDto>();
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
