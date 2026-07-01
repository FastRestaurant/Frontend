namespace El_buen_sabor.Components.Models
{
    public class ImageUploadResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Url { get; set; }
    }
}
