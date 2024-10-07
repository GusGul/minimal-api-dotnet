using MinimalApi.Domain.Enumerables;

namespace MinimalApi.Domain.DTOs
{
    public record AdministratorDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public Role? Role { get; set; }
    }
}
