namespace El_buen_sabor.Components.Models;


public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Waitress = "Waitress";
    public const string Kitchen = "Kitchen";
    public const string Cashier = "Cashier";

    public const string AdminDisplay = "Administrador";
    public const string WaitressDisplay = "Camarero";
    public const string KitchenDisplay = "Cocinero";
    public const string CashierDisplay = "Cajero";

    // backend → UI
    public static readonly Dictionary<string, string> ToDisplay = new()
    {
        { Admin, AdminDisplay },
        { Waitress, WaitressDisplay },
        { Kitchen, KitchenDisplay },
        { Cashier, CashierDisplay }
    };

    // UI → backend  👈 ESTE ES EL QUE TE FALTABA
    public static readonly Dictionary<string, string> ToRole = new()
    {
        { AdminDisplay, Admin },
        { WaitressDisplay, Waitress },
        { KitchenDisplay, Kitchen },
        { CashierDisplay, Cashier }
    };
}
public static class OrderStatuses
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string ReadyToClose = "ReadyToClose";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";
}

public static class TableStatuses
{
    public const string Free = "Libre";
    public const string Occupied = "Ocupada";
    public const string Waiting = "Esperando";
    public const string Disabled = "Deshabilitada";
}

public static class ProductTypes
{
    public const string Dish = "Dish";
    public const string Drink = "Drink";
}
