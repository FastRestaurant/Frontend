namespace El_buen_sabor.Components.Service
   
{
    using Components.Models;
    using Components.Interface;
    public class TableService : ITableService
    {
        public Task<List<Table>> GetTablesAsync()
        {
            return Task.FromResult(new List<Table>
        {
            new() { Id = 1, Name = "Mesa 1", Avaible = false },
            new() { Id = 2, Name = "Mesa 2", Avaible = true },
            new() { Id = 3, Name = "Mesa 3", Avaible = false },
            new() { Id = 4, Name = "Mesa 4", Avaible = true },
            new() { Id = 5, Name = "Mesa 5", Avaible = false },
            new() { Id = 6, Name = "Mesa 6", Avaible = false }
        });
        }

        public Task<List<Order>> GetOrdersByTableAsync(int tableId)
        {
            var orders = tableId switch
            {
                1 => new List<Order>
        {
            new()
            {
                Id = 101,
                Status = "Entregado",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 1, Name = "Pizza", Price = 12000 },
                        Cantidad = 2
                    },
                    new()
                    {
                        Producto = new Product { Id = 2, Name = "Coca Cola", Price = 3000 },
                        Cantidad = 1
                    }
                ]
            }
        },

                2 => new List<Order>
        {
            new()
            {
                Id = 201,
                Status = "En preparación",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 3, Name = "Hamburguesa", Price = 8500 },
                        Cantidad = 2
                    }
                ]
            },

            new()
            {
                Id = 202,
                Status = "Pendiente",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 4, Name = "Papas Fritas", Price = 4000 },
                        Cantidad = 1
                    }
                ]
            }
        },

                3 => new List<Order>
        {
            new()
            {
                Id = 301,
                Status = "Entregado",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 5, Name = "Milanesa", Price = 10000 },
                        Cantidad = 1
                    }
                ]
            },

            new()
            {
                Id = 302,
                Status = "Entregado",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 6, Name = "Agua", Price = 2000 },
                        Cantidad = 2
                    }
                ]
            },

            new()
            {
                Id = 303,
                Status = "En preparación",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 7, Name = "Flan", Price = 3500 },
                        Cantidad = 1
                    }
                ]
            }
        },

                4 => new List<Order>(),

                5 => new List<Order>
        {
            new()
            {
                Id = 501,
                Status = "Pendiente",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 8, Name = "Empanada", Price = 1500 },
                        Cantidad = 6
                    }
                ]
            },

            new()
            {
                Id = 502,
                Status = "En preparación",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 9, Name = "Sprite", Price = 2800 },
                        Cantidad = 2
                    }
                ]
            }
        },

                6 => new List<Order>
        {
            new()
            {
                Id = 601,
                Status = "Pendiente",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 10, Name = "Lomo Completo", Price = 14000 },
                        Cantidad = 1
                    }
                ]
            },

            new()
            {
                Id = 602,
                Status = "En preparación",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 11, Name = "Cerveza", Price = 4500 },
                        Cantidad = 3
                    }
                ]
            },

            new()
            {
                Id = 603,
                Status = "Entregado",
                OrderItems =
                [
                    new()
                    {
                        Producto = new Product { Id = 12, Name = "Helado", Price = 3000 },
                        Cantidad = 2
                    }
                ]
            }
        },

                _ => new List<Order>()
            };

            return Task.FromResult(orders);
        }






    }
}
