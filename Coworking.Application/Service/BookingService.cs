using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Coworking.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Service;

public class BookingService(IApplicationDbContext context) : IBookingService
{
    public async Task<ErrorOr<Domain.Entity.Booking>> CreateBookingAsync(CreateBookingDto dto)
    {
        // 1. Проверяем базовую логику времени
        if (dto.StartTime >= dto.EndTime)
        {
            return Error.Validation("Booking.InvalidTime", "Время начала не может быть позже или равно времени окончания.");
        }

        if (dto.StartTime < DateTime.UtcNow)
        {
            return Error.Validation("Booking.PastTime", "Нельзя забронировать время в прошлом.");
        }

        // 2. Ищем комнату в базе
        var room = await context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
        {
            return Error.NotFound("Booking.RoomNotFound", "Указанная комната не найдена.");
        }

        // 3. ПРОВЕРКА ПЕРЕСЕЧЕНИЯ ВРЕМЕНИ
        bool isOccupied = await context.Bookings
            .AnyAsync(b => b.RoomId == dto.RoomId &&
                           b.Status != BookingStatus.Canceled && // Отмененные брони не мешают
                           dto.StartTime < b.EndTime && 
                           dto.EndTime > b.StartTime);

        if (isOccupied)
        {
            return Error.Conflict("Booking.TimeConflict", "К сожалению, это время в данной комнате уже занято.");
        }

        // 4. Считаем итоговую стоимость
        var duration = dto.EndTime - dto.StartTime;
        if (duration.TotalHours < 1)
        {
            return Error.Validation("Booking.TooShort", "Минимальное время бронирования — 1 час.");
        }
        var totalHours = (decimal)(dto.EndTime - dto.StartTime).TotalHours;
        var totalPrice = room.PricePerHour * (decimal)duration.TotalHours;  
        

        // 5. Создаем бронь
        var booking = new Domain.Entity.Booking
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            RoomId = dto.RoomId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending // Ждет подтверждения
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        return booking; // Возвращаем созданный объект (ErrorOr сам упакует его в Success)
    }
}