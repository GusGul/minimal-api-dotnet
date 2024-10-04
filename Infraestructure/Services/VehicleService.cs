using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infraestructure.Db;

namespace MinimalApi.Infraestructure.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly Context _context;

        public VehicleService(Context context)
        {
            _context = context;
        }

        public Vehicle Create(Vehicle vehicle)
        {
            throw new NotImplementedException();
        }

        public List<Vehicle> GetAll(int page = 1, string? name = null, string? brand = null)
        {
            throw new NotImplementedException();
        }

        public Vehicle GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Vehicle Update(Vehicle vehicle)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
