namespace El_buen_sabor.Components.Models
{
    public class TableDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public int SeatCount { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
    }
}
