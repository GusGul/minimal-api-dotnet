using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;

namespace MinimalApi.Domain.Interfaces
{
    public interface IAdministratorService
    {
        Administrator? Login(LoginDTO loginDTO);
        List<Administrator> GetAll(int page = 1);
        Administrator? GetById(int id);
        void Create(Administrator administrator);
    }
}
