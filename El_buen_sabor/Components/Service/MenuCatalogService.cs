using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using El_buen_sabor.Components.Models;
using Microsoft.AspNetCore.Components.Forms;

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
            var result = await GetDishesPageAsync();
            return result.Items;
        }

        public async Task<PagedResponseDto<DishDto>> GetDishesPageAsync(int pageNumber = 1, int pageSize = 10, string? search = null, bool? available = null, string? sort = null)
        {
            try
            {
                var query = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(search))
                    query.Add($"search={Uri.EscapeDataString(search.Trim())}");

                if (available.HasValue)
                    query.Add($"available={available.Value.ToString().ToLowerInvariant()}");

                if (!string.IsNullOrWhiteSpace(sort))
                    query.Add($"sort={Uri.EscapeDataString(sort)}");

                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/v1/dishes?{string.Join("&", query)}");
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener platos. Código: {response.StatusCode}");
                    return new PagedResponseDto<DishDto>();
                }

                var result = await response.Content.ReadFromJsonAsync<PagedResponseDto<DishDto>>();
                return result ?? new PagedResponseDto<DishDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener platos: {ex.Message}");
                return new PagedResponseDto<DishDto>();
            }
        }

        public async Task<bool> CreateDishAsync(CreateDishDto dto)
        {
            return await CreateDishWithResultAsync(dto) is not null;
        }

        public async Task<DishDto?> CreateDishWithResultAsync(CreateDishDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/dishes");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<DishDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear plato: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateDishAsync(UpdateDishDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/v1/dishes/{dto.Id}");
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
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/v1/dishes/{id}");
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
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/v1/categories");
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

        public async Task<CategoryUsageDto> GetCategoryUsageAsync(Guid categoryId)
        {
            var dishesTask = GetCategoryProductCountAsync($"api/v1/dishes/category/{categoryId}?pageNumber=1&pageSize=1");
            var drinksTask = GetCategoryProductCountAsync($"api/v1/drinks/category/{categoryId}?pageNumber=1&pageSize=1");
            await Task.WhenAll(dishesTask, drinksTask);

            return new CategoryUsageDto
            {
                CategoryId = categoryId,
                DishCount = await dishesTask,
                DrinkCount = await drinksTask
            };
        }

        public async Task<OperationResultDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/categories");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                return await BuildResultAsync(response, "Categoría creada.", "No se pudo crear la categoría.");
            }
            catch
            {
                return Fail("No se pudo crear la categoría. Intentá nuevamente.");
            }
        }

        public async Task<OperationResultDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/v1/categories/{id}");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                return await BuildResultAsync(response, "Categoría actualizada.", "No se pudo actualizar la categoría.");
            }
            catch
            {
                return Fail("No se pudo actualizar la categoría. Intentá nuevamente.");
            }
        }

        public async Task<OperationResultDto> DeleteCategoryAsync(Guid id)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/v1/categories/{id}");
                using var response = await _http.SendAsync(request);
                return await BuildResultAsync(response, "Categoría eliminada.", "No se pudo eliminar la categoría.");
            }
            catch
            {
                return Fail("No se pudo eliminar la categoría. Intentá nuevamente.");
            }
        }

        public async Task<List<DrinkDto>> GetDrinksAsync()
        {
            var result = await GetDrinksPageAsync();
            return result.Items;
        }

        public async Task<PagedResponseDto<DrinkDto>> GetDrinksPageAsync(int pageNumber = 1, int pageSize = 10, string? search = null, bool? available = null, string? sort = null)
        {
            try
            {
                var query = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(search))
                    query.Add($"search={Uri.EscapeDataString(search.Trim())}");

                if (available.HasValue)
                    query.Add($"available={available.Value.ToString().ToLowerInvariant()}");

                if (!string.IsNullOrWhiteSpace(sort))
                    query.Add($"sort={Uri.EscapeDataString(sort)}");

                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/v1/drinks?{string.Join("&", query)}");
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener bebidas. Código: {response.StatusCode}");
                    return new PagedResponseDto<DrinkDto>();
                }

                var result = await response.Content.ReadFromJsonAsync<PagedResponseDto<DrinkDto>>();
                return result ?? new PagedResponseDto<DrinkDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener bebidas: {ex.Message}");
                return new PagedResponseDto<DrinkDto>();
            }
        }

        public async Task<bool> CreateDrinkAsync(CreateDrinkDto dto)
        {
            return await CreateDrinkWithResultAsync(dto) is not null;
        }

        public async Task<DrinkDto?> CreateDrinkWithResultAsync(CreateDrinkDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/drinks");
                request.Content = JsonContent.Create(dto);
                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<DrinkDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear bebida: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> UploadImageAsync(IBrowserFile file)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/images");
                using var content = new MultipartFormDataContent();
                await using var stream = file.OpenReadStream(5 * 1024 * 1024);
                using var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.Name);
                request.Content = content;

                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
                return result?.Url;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir imagen: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateDrinkAsync(Guid id, UpdateDrinkDto dto)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/v1/drinks/{id}");
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
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/v1/drinks/{id}");
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

        private async Task<int> GetCategoryProductCountAsync(string url)
        {
            try
            {
                using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, url);
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return 0;

                var result = await response.Content.ReadFromJsonAsync<PagedResponseDto<object>>();
                return result?.TotalItems ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private static async Task<OperationResultDto> BuildResultAsync(HttpResponseMessage response, string successMessage, string fallbackMessage)
        {
            if (response.IsSuccessStatusCode)
                return new OperationResultDto { Success = true, Message = successMessage };

            return new OperationResultDto
            {
                Success = false,
                Message = await ReadErrorMessageAsync(response, fallbackMessage)
            };
        }

        private static OperationResultDto Fail(string message) => new()
        {
            Success = false,
            Message = message
        };

        private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, string fallbackMessage)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return fallbackMessage;

            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return string.IsNullOrWhiteSpace(error?.Message) ? fallbackMessage : error.Message;
            }
            catch (JsonException)
            {
                return fallbackMessage;
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

        private sealed class ImageUploadResponse
        {
            public string? Url { get; set; }
        }

        private sealed class ApiErrorResponse
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
