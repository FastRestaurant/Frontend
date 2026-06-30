namespace El_buen_sabor.Components.Models
{
    public class UserPageResponseDto
    {
        public List<UserDto> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalUsers { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public Dictionary<string, int> RoleCounts { get; set; } = new();
    }
}
