namespace Coworking.Application.DTOs;

public class CreateBookingDto
{
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}