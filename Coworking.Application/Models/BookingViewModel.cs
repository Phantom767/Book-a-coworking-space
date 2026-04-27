using Coworking.Application.DTOs;
using Coworking.Domain.Enums;

namespace Coworking.Application.Models;

public class BookingViewModel
{
    public Guid   Id            { get; set; }
    public string RoomName      { get; set; }
    public RoomType RoomType      { get; set; }
    public DateTime StartTime   { get; set; }
    public DateTime EndTime     { get; set; }
    public int    DurationHours { get; set; }
    public string DateDisplay   { get; set; }
    public bool   IsNow         { get; set; }
    public string CountdownDisplay { get; set; }
    public BookingStatus Status { get; set; }

    public BookingViewModel(BookingDto booking, DateTime now)
    {
        Id       = booking.Id;
        RoomName = booking.RoomName ?? "Неизвестный зал";
        RoomType = booking.Type;

        StartTime     = booking.StartTime;
        EndTime       = booking.EndTime;
        DurationHours = (int)Math.Round((EndTime - StartTime).TotalHours);

        DateDisplay = StartTime.Date == now.Date
            ? $"Сегодня, {StartTime:HH:mm} – {EndTime:HH:mm}"
            : StartTime.Date == now.Date.AddDays(1)
                ? $"Завтра, {StartTime:HH:mm} – {EndTime:HH:mm}"
                : $"{StartTime:dd MMM, HH:mm} – {EndTime:HH:mm}";

        IsNow  = StartTime <= now && EndTime > now;
        Status = EndTime <= now
            ? BookingStatus.Confirmed
            : IsNow
                ? BookingStatus.Active
                : BookingStatus.Upcoming;

        CountdownDisplay = BuildCountdown(now);
    }

    private string BuildCountdown(DateTime now)
    {
        var diff = IsNow ? EndTime - now : StartTime - now;

        if (diff.TotalDays >= 1)
            return $"{(int)diff.TotalDays}д {diff.Hours}ч";

        if (diff.TotalHours >= 1)
            return $"{diff.Hours}ч {diff.Minutes}м";

        return $"{diff.Minutes}м";
    }
}