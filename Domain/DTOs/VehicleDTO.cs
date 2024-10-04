namespace MinimalApi.Domain.DTOs
{
    public record VehicleDTO
    {
        public required string Name { get; set; }
        public required string Brand { get; set; }
        public required int Year { get; set; }
    }
}
