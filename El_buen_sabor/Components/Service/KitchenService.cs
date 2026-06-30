using System.Net.Http.Headers;
using System.Net.Http.Json;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Service
{
    public sealed class KitchenService
    {
        private readonly HttpClient _http;
        private readonly AuthSessionService _authSession;

        public KitchenService(HttpClient http, AuthSessionService authSession)
        {
            _http = http;
            _authSession = authSession;
        }

        public async Task<List<KitchenQueueItemDto>> GetCookingItemsAsync()
        {
            return await GetItemsAsync("api/v1/kitchenOrders/queue");
        }

        public async Task<List<KitchenQueueItemDto>> GetWaitingItemsAsync()
        {
            return await GetItemsAsync("api/v1/kitchenOrders/queue-waiting-items");
        }

        public async Task<bool> CompleteItemAsync(Guid itemId)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/kitchenOrders/items/{itemId}/complete");
                using var response = await SendAuthorizedAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<KitchenConfigurationDto?> GetConfigurationAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/kitchenOrders/configuration");
                using var response = await SendAuthorizedAsync(request);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<KitchenConfigurationDto>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<OperationResultDto> UpdateConfigurationAsync(KitchenConfigurationDto configuration)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Patch, "api/v1/kitchenOrders/max-concurrent-dishes")
                {
                    Content = JsonContent.Create(configuration)
                };

                using var response = await SendAuthorizedAsync(request);
                return response.IsSuccessStatusCode
                    ? new OperationResultDto { Success = true, Message = "Capacidad de cocina actualizada." }
                    : new OperationResultDto { Success = false, Message = "No se pudo actualizar la capacidad de cocina." };
            }
            catch
            {
                return new OperationResultDto { Success = false, Message = "No se pudo actualizar la capacidad de cocina. Intentá nuevamente." };
            }
        }

        private async Task<List<KitchenQueueItemDto>> GetItemsAsync(string url)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await SendAuthorizedAsync(request);

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException("No se pudo cargar la cola de cocina.");

                return await response.Content.ReadFromJsonAsync<List<KitchenQueueItemDto>>() ?? [];
            }
            catch
            {
                throw;
            }
        }

        private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpRequestMessage request)
        {
            var token = await _authSession.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _http.SendAsync(request);
        }
    }
}
