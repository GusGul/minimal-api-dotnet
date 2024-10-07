using MinimalApi.Domain.Enumerables;

namespace minimal_api.ModelViews
{
    public record AdministratorModelView
    {
        public required int Id { get; set; }
        public required string Email { get; set; }
        public Role? Role { get; set; }
    }
}
