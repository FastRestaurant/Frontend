namespace El_buen_sabor.Components.Models
{
    public class TableRequestDto
    {
        public string Number { get; set; } = string.Empty;
        public int SeatCount { get; set; } = 4;
        public string Location { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
    }
}
