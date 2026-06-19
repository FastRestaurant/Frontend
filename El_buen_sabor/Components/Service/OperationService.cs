using El_buen_sabor.Components.Interface;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Service
{
    public class OperationService : IOperationService
    {
        private readonly ITableService _tableService;
        private readonly List<OrderFromTable> items = [];
        private Table? table;

        public OperationService(ITableService tableService)
        {
            _tableService = tableService;
        }

        public event Action? OnChange;

        public void AddProduct(Product product)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == product.Id);

            if (item == null)
            {
                items.Add(new OrderFromTable
                {
                    Producto = product,
                    Cantidad = 1
                });
            }
            else
            {
                item.Cantidad++;
            }

            OnChange?.Invoke();
        }

        public List<OrderFromTable> GetItems() => items;

        public Table? GetTable() => table;

        public void RemoveProduct(Guid productId)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);
            if (item != null)
            {
                items.Remove(item);
                OnChange?.Invoke();
            }
        }

        public void Clear()
        {
            items.Clear();
            OnChange?.Invoke();
        }

        public void IncreaseQuantity(Guid productId)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);
            if (item != null)
            {
                item.Cantidad++;
                OnChange?.Invoke();
            }
        }

        public void DecreaseQuantity(Guid productId)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);
            if (item != null && item.Cantidad > 1)
            {
                item.Cantidad--;
                OnChange?.Invoke();
            }
        }

        public void SetTable(Table selectedTable)
        {
            table = selectedTable;
            OnChange?.Invoke();
        }

        public void ClearTable()
        {
            table = null;
            items.Clear();
            OnChange?.Invoke();
        }

        public void AddNote(Guid productId, string note)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);
            if (item != null)
            {
                item.Notes = note;
                OnChange?.Invoke();
            }
        }

        public async Task<OperationResultDto> SendOrderAsync()
        {
            if (table is null)
                return Fail("Seleccioná una mesa antes de confirmar la orden.");

            if (items.Count == 0)
                return Fail("Agregá al menos un producto antes de confirmar la orden.");

            var requestItems = items.Select(item => new CreateOrderItemRequest
            {
                ProductId = item.Producto.Id,
                ProductType = item.Producto.ProductType,
                Quantity = item.Cantidad,
                Notes = item.Notes
            }).ToList();

            var orders = await _tableService.GetOrdersByTableAsync(table.Id);
            var activeOrder = orders.FirstOrDefault(IsActiveOrder);

            if (activeOrder?.Status == OrderStatuses.ReadyToClose)
                return Fail("La cuenta ya fue solicitada. No se pueden agregar productos.");

            OperationResultDto result;

            if (activeOrder is null)
            {
                result = await _tableService.CreateOrderAsync(new CreateOrderRequest
                {
                    TableId = table.Id,
                    Items = requestItems
                });
            }
            else
            {
                result = new OperationResultDto { Success = true, Message = "Productos agregados correctamente." };

                foreach (var item in requestItems)
                {
                    result = await _tableService.AddItemToOrderAsync(activeOrder.Id, item);
                    if (!result.Success)
                        return result;
                }
            }

            if (result.Success)
            {
                items.Clear();
                OnChange?.Invoke();
            }

            return result;
        }

        public async Task<OperationResultDto> RequestBillAsync()
        {
            var activeOrder = await GetActiveOrderAsync();
            if (activeOrder is null)
                return Fail("No hay una orden activa para esta mesa.");

            if (activeOrder.Status == OrderStatuses.ReadyToClose)
                return new OperationResultDto { Success = true, Message = "La cuenta ya fue solicitada." };

            var result = await _tableService.ChangeOrderStatusAsync(activeOrder.Id, OrderStatuses.ReadyToClose);
            if (result.Success)
            {
                result.Message = "Cuenta solicitada.";
                OnChange?.Invoke();
            }

            return result;
        }

        public async Task<OperationResultDto> ReleaseTableAsync()
        {
            var activeOrder = await GetActiveOrderAsync();
            if (activeOrder is null)
                return Fail("No hay una orden pendiente para liberar.");

            if (activeOrder.Status != OrderStatuses.ReadyToClose)
                return Fail("Primero solicitá la cuenta.");

            var result = await _tableService.ChangeOrderStatusAsync(activeOrder.Id, OrderStatuses.Closed);
            if (result.Success)
            {
                result.Message = "Mesa liberada.";
                OnChange?.Invoke();
            }

            return result;
        }

        private async Task<Order?> GetActiveOrderAsync()
        {
            if (table is null)
                return null;

            var orders = await _tableService.GetOrdersByTableAsync(table.Id);
            return orders.FirstOrDefault(IsActiveOrder);
        }

        private static bool IsActiveOrder(Order order)
            => order.Status is not OrderStatuses.Closed and not OrderStatuses.Cancelled;

        private static OperationResultDto Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
