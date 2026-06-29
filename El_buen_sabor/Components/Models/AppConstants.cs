namespace El_buen_sabor.Components.Models;

public static class AppRoles
{
    public const string Admin = "ADMIN";
    public const string Kitchen = "KITCHEN";
    public const string Cashier = "CASHIER";
    public const string Waitress = "WAITRESS";

    public const string AdminDisplay = "ADMINISTRADOR";
    public const string KitchenDisplay = "COCINERO";
    public const string CashierDisplay = "CAJERO";
    public const string WaitressDisplay = "CAMARERO";
}

public static class OrderStatuses
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string ReadyToClose = "ReadyToClose";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";
}

public static class OrderItemStatuses
{
    public const string Pending = "Pending";
    public const string SentToKitchen = "SentToKitchen";
    public const string Ready = "Ready";
    public const string Delivered = "Delivered";
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
