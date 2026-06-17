namespace El_buen_sabor.Components.Service
{
    using Components.Models;
    using El_buen_sabor.Components.Interface;

    public class OperationService : IOperationService
    {
        private List<OrderFromTable> items { get; } = [];

        private Table? table;

        public event Action? OnChange;

        public void AddProduct(Product product)
        {
            var item = items.FirstOrDefault(x =>
                x.Producto.Id == product.Id);

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

            Console.WriteLine($"Suscriptores: {OnChange?.GetInvocationList().Length ?? 0}");

            if (OnChange == null)
            {
                throw new Exception("No hay suscriptores");
            }

            OnChange?.Invoke();
        }

        public List<OrderFromTable> GetItems()
        {
            return items;
        }

        public Table? GetTable()
        {
            return table;
        }

        public void RemoveProduct(int productId)
        {
            var item = items.FirstOrDefault(x =>
                x.Producto.Id == productId);

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

        public void IncreaseQuantity(int productId)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);

            if (item != null)
            {
                item.Cantidad++;
                OnChange?.Invoke();
            }
        }

        public void DecreaseQuantity(int productId)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);

            if (item != null && item.Cantidad > 1)
            {
                item.Cantidad--;
                OnChange?.Invoke(); // ante cualquier evento disparo el refresh
            }
        }

        public void SetTable(Table selectedTable)
        {
            table = selectedTable;
            OnChange?.Invoke();
        }

        public void AddNote(int productId, string note)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);
            if (item != null)
            {
                item.Notes = note;
                OnChange?.Invoke();
            }
        }

        public void SendOrder()
        {
            // codigo para conectar con la api que crea la orden 
            // usar los dtos order y orderitem(orderfomtable) no me dejaba cambiar el nombre
            // vamos fede que podes sos groso !!
            OnChange?.Invoke();
        }

    }
}
