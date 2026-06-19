namespace El_buen_sabor.Components.Models
{
    public class Table
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Avaible { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public Guid? ActiveWaiterId { get; set; }
    }
}
