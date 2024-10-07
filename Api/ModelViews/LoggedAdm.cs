using MinimalApi.Domain.Enumerables;

namespace minimal_api.ModelViews
{
    public record LoggedAdm
    {
        public string Email { get; set; }
        public Role Role { get; set; }
        public string Token { get; set; }
    }
}
