namespace El_buen_sabor.Components.Interface
{
    using Components.Models;
    public interface IOperationService
    {

        event Action? OnChange;
        void AddProduct(Product product);
        List<OrderFromTable> GetItems();
        Table? GetTable();
        void SetTable(Table t);
        void RemoveProduct(int productId);
        void Clear();
        void IncreaseQuantity(int productId);
        void DecreaseQuantity(int productId);
        public void AddNote(int productId, string note);
        public void SendOrder();


    }
}
