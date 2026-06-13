using Coworking.Domain.Enums;

namespace Coworking.Application.DTOs;

public class BookingAdminDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public string StatusBadgeClass => Status switch
    {
        BookingStatus.Pending   => "warning",
        BookingStatus.Confirmed => "success",
        BookingStatus.Cancelled => "danger",
        BookingStatus.Completed => "secondary",
        _ => "secondary"
    };

    public string StatusLabel => Status switch
    {
        BookingStatus.Pending   => "Ожидает",
        BookingStatus.Confirmed => "Подтверждена",
        BookingStatus.Cancelled => "Отменена",
        BookingStatus.Completed => "Завершена",
        _ => "—"
    };

    public double DurationHours => (EndTime - StartTime).TotalHours;
}