namespace El_buen_sabor.Components.Models
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;

        // 🔥 ESTE es el importante
        public string Name { get; set; } = string.Empty;

        public string NormalizedName { get; set; } = string.Empty;
    }
}