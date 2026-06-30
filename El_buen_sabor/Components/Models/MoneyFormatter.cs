using System.Globalization;

namespace El_buen_sabor.Components.Models;

public static class MoneyFormatter
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-AR");

    public static string Format(decimal value) => $"${value.ToString("N0", Culture)}";
}
