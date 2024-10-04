using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infraestructure.Db;
using System.Runtime.Versioning;

namespace MinimalApi.Infraestructure.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly Context _context;

        public VehicleService(Context context)
        {
            _context = context;
        }

        public void Create(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
        }

        public List<Vehicle> GetAll(int page = 1, string? name = null, string? brand = null)
        {
            var query = _context.Vehicles.AsQueryable();
            if(!string.IsNullOrEmpty(name))
            {
                query = query.Where(v => EF.Functions.Like(v.Name.ToLower(), $"%{name}%"));
            }

            int pageSize = 10;

            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return query.ToList();
        }

        public Vehicle? GetById(int id)
        {
            return _context.Vehicles.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Update(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            _context.Vehicles.Remove(_context.Vehicles.Find(id));
            _context.SaveChanges();
        }
    }
}
