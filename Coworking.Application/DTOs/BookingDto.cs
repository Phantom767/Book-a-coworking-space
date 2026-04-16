using Coworking.Domain.Entity;
using Coworking.Domain.Enums;

namespace Coworking.Application.DTOs;

public class BookingDto : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    public required string RoomName { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice { get; set; }
}