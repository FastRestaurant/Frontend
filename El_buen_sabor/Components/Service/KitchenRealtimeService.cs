using El_buen_sabor.Components.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace El_buen_sabor.Components.Service
{
    public sealed class KitchenRealtimeService : IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly AuthSessionService _authSession;
        private readonly ILogger<KitchenRealtimeService> _logger;
        private HubConnection? _connection;

        public event Func<KitchenQueueSnapshotDto, Task>? QueueChanged;
        public event Func<Task>? Reconnected;

        public KitchenRealtimeService(
            IConfiguration configuration,
            AuthSessionService authSession,
            ILogger<KitchenRealtimeService> logger)
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
                _logger.LogWarning(ex, "No se pudo iniciar la conexion realtime de cocina.");
            }
        }

        private HubConnection CreateConnection()
        {
            var kitchenBaseUrl = _configuration["ExternalServices:Kitchen:BaseUrl"] ?? "https://localhost:7200/";
            var hubUrl = new Uri(new Uri(kitchenBaseUrl), "hubs/kitchen");

            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = async () => await _authSession.GetTokenAsync();
                })
                .WithAutomaticReconnect()
                .Build();

            connection.On<KitchenQueueSnapshotDto>("QueueChanged", async snapshot =>
            {
                if (QueueChanged is not null)
                    await QueueChanged.Invoke(snapshot);
            });

            connection.Closed += ex =>
            {
                if (ex is not null)
                    _logger.LogWarning(ex, "La conexion realtime de cocina se cerro.");

                return Task.CompletedTask;
            };

            connection.Reconnected += async _ =>
            {
                if (Reconnected is not null)
                    await Reconnected.Invoke();
            };

            return connection;
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }
    }
}
