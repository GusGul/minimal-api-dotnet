using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infraestructure.Db;

namespace MinimalApi.Infrastructure.Services
{
    public class AdministratorService : IAdministratorService
    {
        private readonly Context _context;

        public AdministratorService(Context context)
        {
            _context = context;
        }

        Administrator? Login(LoginDTO loginDTO)
        {
            if (_context.Administrators.Where(adm => adm.Email == loginDTO.Email && adm.Password == loginDTO.Password).Any()
                return _context.Administrators.Where(adm => adm.Email == loginDTO.Email && adm.Password == loginDTO.Password).FirstOrDefault();
            else
                return null;
        }
    }
}
