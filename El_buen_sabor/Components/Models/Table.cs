namespace El_buen_sabor.Components.Models
{
    public class Table
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public int SeatCount { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool Avaible { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public int SeatCount { get; set; }
        public Guid? ActiveWaiterId { get; set; }
        public decimal? PositionX { get; set; }
        public decimal? PositionY { get; set; }
    }
}
