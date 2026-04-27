namespace Coworking.Domain.Enums;

public enum BookingStatus
{
    Upcoming,
    Active,
    Pending,   // В ожидании подтверждения
    Confirmed, // Подтверждено
    Cancelled   // Отменено
}