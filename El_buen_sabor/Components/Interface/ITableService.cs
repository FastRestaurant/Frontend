using System.Threading.Tasks;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Interface
{
    public interface ITableService
    {
        bool LastOrdersLoadFailed { get; }

        Task<List<Table>> GetTablesAsync();

        Task<List<Order>> GetOrdersByTableAsync(Guid tableId);

        Task<List<OrderRealtimeEventDto>> GetReadyDeliveryOrdersAsync();

        Task<TableOrdersSummaryDto?> GetActiveOrdersSummaryByTableAsync(Guid tableId);

        Task<OperationResultDto> CreateOrderAsync(CreateOrderRequest request);

        Task<OperationResultDto> AddItemToOrderAsync(Guid orderId, CreateOrderItemRequest request);

        Task<OperationResultDto> MarkOrderItemDeliveredAsync(Guid orderId, Guid itemId);

        Task<OperationResultDto> ChangeOrderStatusAsync(Guid orderId, string newStatus);
    }
}
