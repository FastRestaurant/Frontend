using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using El_buen_sabor.Components.Interface;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Service
{
    public class TableService : ITableService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public TableService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<List<Table>> GetTablesAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/tables?page=1&pageSize=100");
                using var response = await SendAuthorizedAsync(request);

                if (!response.IsSuccessStatusCode)
                    return [];

                var page = await response.Content.ReadFromJsonAsync<PagedResponseDto<TableDto>>() ?? new PagedResponseDto<TableDto>();

                return page.Items.Select(table => new Table
                {
                    Id = table.Id,
                    Name = string.IsNullOrWhiteSpace(table.Number) ? "Mesa" : $"Mesa {table.Number}",
                    Number = table.Number,
                    SeatCount = table.SeatCount,
                    Location = table.Location,
                    IsEnabled = table.IsEnabled,
                    OperationalStatus = table.OperationalStatus,
                    Avaible = table.OperationalStatus is TableStatuses.Occupied or TableStatuses.Waiting,
                    ActiveWaiterId = table.ActiveWaiterId
                }).ToList();
            }
            catch
            {
                return [];
            }
        }

        public async Task<List<Order>> GetOrdersByTableAsync(Guid tableId)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/orders/table/{tableId}?page=1&pageSize=20");
                using var response = await SendAuthorizedAsync(request);

                if (!response.IsSuccessStatusCode)
                    return [];

                var page = await response.Content.ReadFromJsonAsync<PagedResponseDto<Order>>() ?? new PagedResponseDto<Order>();
                var orders = new List<Order>();

                foreach (var summary in page.Items)
                    orders.Add(await GetOrderDetailsAsync(summary.Id) ?? summary);

                return orders;
            }
            catch
            {
                return [];
            }
        }

        public async Task<OperationResultDto> CreateOrderAsync(CreateOrderRequest order)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/orders")
                {
                    Content = JsonContent.Create(order)
                };

                using var response = await SendAuthorizedAsync(request);
                return await BuildResultAsync(response, "Orden enviada correctamente.", "No se pudo crear la orden.");
            }
            catch
            {
                return Fail("No se pudo crear la orden. Intentá nuevamente.");
            }
        }

        public async Task<OperationResultDto> AddItemToOrderAsync(Guid orderId, CreateOrderItemRequest item)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/orders/{orderId}/items")
                {
                    Content = JsonContent.Create(item)
                };

                using var response = await SendAuthorizedAsync(request);
                return await BuildResultAsync(response, "Producto agregado correctamente.", "No se pudo agregar el producto a la orden.");
            }
            catch
            {
                return Fail("No se pudo agregar el producto. Intentá nuevamente.");
            }
        }

        public async Task<OperationResultDto> ChangeOrderStatusAsync(Guid orderId, string newStatus)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/orders/{orderId}/status")
                {
                    Content = JsonContent.Create(new { NewStatus = newStatus })
                };

                using var response = await SendAuthorizedAsync(request);
                return await BuildResultAsync(response, "Estado actualizado correctamente.", "No se pudo actualizar la mesa.");
            }
            catch
            {
                return Fail("No se pudo actualizar la mesa. Intentá nuevamente.");
            }
        }

        private async Task<Order?> GetOrderDetailsAsync(Guid orderId)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/orders/{orderId}");
                using var response = await SendAuthorizedAsync(request);

                if (!response.IsSuccessStatusCode)
                    return null;

                var detail = await response.Content.ReadFromJsonAsync<OrderDetailsDto>();
                if (detail is null)
                    return null;

                return new Order
                {
                    Id = detail.Id,
                    TableId = detail.TableId,
                    TableNumber = detail.TableNumber,
                    Status = detail.Status,
                    Total = detail.Total,
                    CreatedAt = detail.CreatedAt,
                    ItemCount = detail.Items.Count,
                    OrderItems = detail.Items.Select(item => new OrderFromTable
                    {
                        Producto = new Product
                        {
                            Id = item.ProductId,
                            Name = item.ProductNameSnapshot,
                            Price = item.UnitPriceSnapshot,
                            ProductType = item.ProductType,
                            Available = true
                        },
                        Cantidad = item.Quantity,
                        Notes = item.Notes
                    }).ToList()
                };
            }
            catch
            {
                return null;
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

        private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpRequestMessage request)
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _http.SendAsync(request);
        }

        private sealed class OrderDetailsDto
        {
            public Guid Id { get; set; }
            public Guid TableId { get; set; }
            public string TableNumber { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<OrderItemDto> Items { get; set; } = [];
        }

        private sealed class OrderItemDto
        {
            public Guid ProductId { get; set; }
            public string ProductType { get; set; } = string.Empty;
            public string ProductNameSnapshot { get; set; } = string.Empty;
            public decimal UnitPriceSnapshot { get; set; }
            public int Quantity { get; set; }
            public string? Notes { get; set; }
        }

        private sealed class ApiErrorResponse
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
