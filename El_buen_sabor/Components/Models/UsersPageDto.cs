namespace El_buen_sabor.Components.Models
{
    public class UsersPageDto
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalUsers { get; set; }
        public List<UserDto> Data { get; set; } = new();
    }
}