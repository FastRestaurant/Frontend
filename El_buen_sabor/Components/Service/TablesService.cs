using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Service
{
    public class TablesService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public TablesService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<PagedResponseDto<TableDto>> GetTablesAsync(int page, int pageSize)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/tables?page={page}&pageSize={pageSize}");
                using var response = await SendAuthorizedAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al obtener mesas. Código: {response.StatusCode}");
                    return new PagedResponseDto<TableDto>();
                }

                return await response.Content.ReadFromJsonAsync<PagedResponseDto<TableDto>>() ?? new PagedResponseDto<TableDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener mesas: {ex.Message}");
                return new PagedResponseDto<TableDto>();
            }
        }

        public async Task<OperationResultDto> CreateTableAsync(TableRequestDto table)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/tables")
                {
                    Content = JsonContent.Create(table)
                };

                using var response = await SendAuthorizedAsync(request);
                return await BuildResultAsync(response, "Mesa registrada correctamente.", "No se pudo crear la mesa.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al crear mesa: {ex.Message}");
                return Fail("No se pudo conectar con Orders para crear la mesa.");
            }
        }

        public async Task<OperationResultDto> UpdateTableAsync(Guid id, TableRequestDto table)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/tables/{id}")
                {
                    Content = JsonContent.Create(table)
                };

                using var response = await SendAuthorizedAsync(request);
                return await BuildResultAsync(response, "Mesa actualizada correctamente.", "No se pudo actualizar la mesa.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al actualizar mesa: {ex.Message}");
                return Fail("No se pudo conectar con Orders para actualizar la mesa.");
            }
        }

        public async Task<OperationResultDto> ToggleTableStatusAsync(Guid id, bool enable)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/tables/{id}/status")
                {
                    Content = JsonContent.Create(new { Enable = enable })
                };

                using var response = await SendAuthorizedAsync(request);
                return await BuildResultAsync(response, "Estado de mesa actualizado correctamente.", "No se pudo cambiar el estado de la mesa.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al cambiar estado de mesa: {ex.Message}");
                return Fail("No se pudo conectar con Orders para cambiar el estado de la mesa.");
            }
        }

        public async Task<OperationResultDto> DeleteTableAsync(Guid id)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/tables/{id}");
                using var response = await SendAuthorizedAsync(request);
                return await BuildResultAsync(response, "Mesa eliminada correctamente.", "No se pudo eliminar la mesa.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al eliminar mesa: {ex.Message}");
                return Fail("No se pudo conectar con Orders para eliminar la mesa.");
            }
        }

        private static async Task<OperationResultDto> BuildResultAsync(HttpResponseMessage response, string successMessage, string fallbackMessage)
        {
            if (response.IsSuccessStatusCode)
            {
                return new OperationResultDto { Success = true, Message = successMessage };
            }

            return Fail(await ReadErrorMessageAsync(response, fallbackMessage));
        }

        private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, string fallbackMessage)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return fallbackMessage;
            }

            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (!string.IsNullOrWhiteSpace(error?.Message))
                {
                    return error.Message;
                }
            }
            catch (JsonException)
            {
                return fallbackMessage;
            }

            return fallbackMessage;
        }

        private static OperationResultDto Fail(string message) => new()
        {
            Success = false,
            Message = message
        };

        private sealed class ApiErrorResponse
        {
            public string Message { get; set; } = string.Empty;
        }

        private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpRequestMessage request)
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _http.SendAsync(request);
        }
    }
}
