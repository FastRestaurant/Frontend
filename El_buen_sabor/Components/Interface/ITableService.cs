using System.Threading.Tasks;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Interface
{
    public interface ITableService
    {
        Task<List<Table>> GetTablesAsync();

        Task<List<Order>> GetOrdersByTableAsync(int tableId);
    }
}
