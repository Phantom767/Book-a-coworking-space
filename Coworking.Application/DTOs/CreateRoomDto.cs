using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Http;

namespace Coworking.Application.DTOs;

public class CreateRoomDto : EntityBase
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal PricePerHour { get; set; }
    public int Capacity { get; set; }
    public IFormFile? Photo { get; set; }
    public List<string>? Amenities { get; set; }
}