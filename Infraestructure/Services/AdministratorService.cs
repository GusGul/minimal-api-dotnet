using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infraestructure.Db;
using System.Xml.Linq;

namespace MinimalApi.Infraestructure.Services
{
    public class AdministratorService : IAdministratorService
    {
        private readonly Context _context;

        public AdministratorService(Context context)
        {
            _context = context;
        }

        public Administrator? Login(LoginDTO loginDTO)
        {
            if (_context.Administrators.Where(adm => adm.Email == loginDTO.Email && adm.Password == loginDTO.Password).Any())
                return _context.Administrators.Where(adm => adm.Email == loginDTO.Email && adm.Password == loginDTO.Password).FirstOrDefault();
            else
                return null;
        }

        public void Create(Administrator administrator)
        {
            _context.Administrators.Add(administrator);
            _context.SaveChanges();
        }

        public List<Administrator> GetAll(int page = 1)
        {
            var query = _context.Administrators.AsQueryable();
            int pageSize = 10;

            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return query.ToList();
        }

        public Administrator? GetById(int id)
        {
            return _context.Administrators.Where(a => a.Id == id).FirstOrDefault();
        }
    }
}
