using MinimalApi.Domain.Entities;

namespace MinimalApi.Infraestructure.Services
{
    public interface IVehicleService
    {
        List<Vehicle> GetAll(int page = 1, string? name = null, string? brand = null);
        Vehicle? GetById(int id);
        void Create(Vehicle vehicle);
        void Update(Vehicle vehicle);
        void Delete(int id);
    }
}
