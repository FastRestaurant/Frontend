using El_buen_sabor.Components.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace El_buen_sabor.Components.Service
{
    public sealed class OrderRealtimeService : IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly AuthSessionService _authSession;
        private readonly ILogger<OrderRealtimeService> _logger;
        private HubConnection? _connection;
        private readonly HashSet<Guid> _delayedOrders = [];
        private readonly HashSet<Guid> _readyOrders = [];
        private readonly List<ReadyDeliveryItemDto> _readyDeliveryItems = [];

        public event Func<OrderRealtimeEventDto, Task>? OrderReadyToClose;
        public event Func<OrderRealtimeEventDto, Task>? OrderDelayed;
        public event Func<OrderRealtimeEventDto, Task>? OrderStatusChanged;
        public event Func<OrderRealtimeEventDto, Task>? OrderItemStatusChanged;
        public event Func<Task>? Reconnected;
        public event Action? DeliveryItemsChanged;

        public bool IsDelayed(Guid orderId) => _delayedOrders.Contains(orderId);

        public bool IsReady(Guid orderId) => _readyOrders.Contains(orderId);

        public IReadOnlyList<ReadyDeliveryItemDto> GetReadyDeliveryItems() => _readyDeliveryItems
            .OrderBy(item => item.ReadyAt)
            .ToArray();

        public void RemoveReadyDeliveryItem(Guid itemId)
        {
            if (_readyDeliveryItems.RemoveAll(item => item.ItemId == itemId) > 0)
                DeliveryItemsChanged?.Invoke();
        }

        public void ReplaceReadyDeliveryItems(IEnumerable<OrderRealtimeEventDto> orders)
        {
            _readyDeliveryItems.Clear();
            foreach (var order in orders)
                ApplyReadyDeliveryItems(order, notify: false);

            DeliveryItemsChanged?.Invoke();
        }

        public OrderRealtimeService(
            IConfiguration configuration,
            AuthSessionService authSession,
            ILogger<OrderRealtimeService> logger)
        {
            _configuration = configuration;
            _authSession = authSession;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            if (_connection?.State is HubConnectionState.Connected or HubConnectionState.Connecting)
                return;

            var token = await _authSession.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
                return;

            _connection ??= CreateConnection();

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo iniciar la conexion realtime de ordenes.");
            }
        }

        private HubConnection CreateConnection()
        {
            var ordersBaseUrl = _configuration["ExternalServices:Orders:BaseUrl"] ?? "https://localhost:7100/";
            var hubUrl = new Uri(new Uri(ordersBaseUrl), "hubs/orders");

            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = async () => await _authSession.GetTokenAsync();
                })
                .WithAutomaticReconnect()
                .Build();

            connection.On<OrderRealtimeEventDto>("OrderReadyToClose", async order =>
            {
                _readyOrders.Add(order.Id);
                _delayedOrders.Remove(order.Id);

                if (OrderReadyToClose is not null)
                    await OrderReadyToClose.Invoke(order);
            });

            connection.On<OrderRealtimeEventDto>("OrderDelayed", async order =>
            {
                _delayedOrders.Add(order.Id);
                _readyOrders.Remove(order.Id);
                MarkReadyItemsDelayed(order.Id);

                if (OrderDelayed is not null)
                    await OrderDelayed.Invoke(order);
            });

            connection.On<OrderRealtimeEventDto>("OrderStatusChanged", async order =>
            {
                if (OrderStatusChanged is not null)
                    await OrderStatusChanged.Invoke(order);
            });

            connection.On<OrderRealtimeEventDto>("OrderItemStatusChanged", async order =>
            {
                ApplyReadyDeliveryItems(order);

                if (OrderItemStatusChanged is not null)
                    await OrderItemStatusChanged.Invoke(order);
            });

            connection.Closed += ex =>
            {
                if (ex is not null)
                    _logger.LogWarning(ex, "La conexion realtime de ordenes se cerro.");

                return Task.CompletedTask;
            };

            connection.Reconnected += async _ =>
            {
                if (Reconnected is not null)
                    await Reconnected.Invoke();
            };

            return connection;
        }

        private void ApplyReadyDeliveryItems(OrderRealtimeEventDto order, bool notify = true)
        {
            var changed = false;

            foreach (var item in order.Items)
            {
                if (string.Equals(item.Status, "Ready", StringComparison.OrdinalIgnoreCase))
                {
                    var existing = _readyDeliveryItems.FirstOrDefault(ready => ready.ItemId == item.Id);
                    if (existing is null)
                    {
                        _readyDeliveryItems.Add(new ReadyDeliveryItemDto
                        {
                            OrderId = order.Id,
                            ItemId = item.Id,
                            TableId = order.TableId,
                            TableNumber = order.TableNumber,
                            ProductName = item.ProductNameSnapshot,
                            Quantity = item.Quantity,
                            ReadyAt = item.ReadyAt ?? DateTime.Now,
                            WasDelayed = _delayedOrders.Contains(order.Id)
                        });
                    }
                    else
                    {
                        existing.TableNumber = order.TableNumber;
                        existing.ProductName = item.ProductNameSnapshot;
                        existing.Quantity = item.Quantity;
                        existing.ReadyAt = item.ReadyAt ?? existing.ReadyAt;
                        existing.WasDelayed = _delayedOrders.Contains(order.Id);
                    }

                    changed = true;
                    continue;
                }

                if (string.Equals(item.Status, "Delivered", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    changed = _readyDeliveryItems.RemoveAll(ready => ready.ItemId == item.Id) > 0 || changed;
                }
            }

            if (changed && notify)
                DeliveryItemsChanged?.Invoke();
        }

        private void MarkReadyItemsDelayed(Guid orderId)
        {
            var changed = false;
            foreach (var item in _readyDeliveryItems.Where(item => item.OrderId == orderId))
            {
                if (item.WasDelayed)
                    continue;

                item.WasDelayed = true;
                changed = true;
            }

            if (changed)
                DeliveryItemsChanged?.Invoke();
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }
    }
}
