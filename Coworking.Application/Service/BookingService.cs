using AutoMapper;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Coworking.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coworking.Application.Service;

public class BookingService(
    IApplicationDbContext context, 
    IMapper mapper,
    ILogger<BookingService> logger) : IBookingService
{
    public async Task<ErrorOr<BookingDto>> CreateBookingAsync(CreateBookingDto dto)
    {
        logger.LogInformation("Создание бронирования: комната {RoomId}, начало {StartTime}, конец {EndTime}", 
            dto.RoomId, dto.StartTime, dto.EndTime);
        
        Console.WriteLine($"Start: {dto.StartTime} | End: {dto.EndTime}");
        Console.WriteLine($"Now UTC: {DateTime.UtcNow}");
        // 1. Проверяем базовую логику времени
        if (dto.StartTime >= dto.EndTime)
        {
            logger.LogWarning("Ошибка валидации времени: начало {StartTime} >= конца {EndTime}", 
                dto.StartTime, dto.EndTime);
            return Error.Validation("Booking.InvalidTime", "Время начала не может быть позже или равно времени окончания.");
        }

        if (dto.StartTime < DateTime.UtcNow)
        {
            logger.LogWarning("Попытка бронирования прошедшего времени: {StartTime}", dto.StartTime);
            return Error.Validation("Booking.PastTime", "Нельзя забронировать время в прошлом.");
        }

        // 2. Ищем комнату в базе
        var room = await context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
        {
            logger.LogError("Комната не найдена: {RoomId}", dto.RoomId);
            return Error.NotFound("Booking.RoomNotFound", "Указанная комната не найдена.");
        }

        // 3. ПРОВЕРКА ПЕРЕСЕЧЕНИЯ ВРЕМЕНИ
        bool isOccupied = await context.Bookings
            .AnyAsync(b => b.RoomId == dto.RoomId &&
                           b.Status != BookingStatus.Cancelled && // Отмененные брони не мешают
                           dto.StartTime < b.EndTime && 
                           dto.EndTime > b.StartTime);

        if (isOccupied)
        {
            logger.LogWarning("Конфликт времени при бронировании комнаты {RoomId} с {StartTime} по {EndTime}", 
                dto.RoomId, dto.StartTime, dto.EndTime);
            return Error.Conflict("Booking.TimeConflict", "К сожалению, это время в данной комнате уже занято.");
        }

        // 4. Считаем итоговую стоимость
        var duration = dto.EndTime - dto.StartTime;
        if (duration.TotalHours < 1)
        {
            logger.LogWarning("Продолжительность бронирования слишком короткая: {Hours} часов", duration.TotalHours);
            return Error.Validation("Booking.TooShort", "Минимальное время бронирования — 1 час.");
        }
        // var totalHours = (decimal)(dto. EndTime - dto. StartTime). TotalHours;
        var totalPrice = room.PricePerHour * (decimal)duration.TotalHours;  
        

        // 5. Создаем бронь
         var booking = mapper.Map<Booking>(dto);
             booking.Id = Guid.NewGuid();
             booking.RoomName = room.Name;  // Устанавливаем имя комнаты
             booking.TotalPrice = totalPrice;
             booking.Status = BookingStatus.Pending; // Ждет подтверждения

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        logger.LogInformation("Бронирование успешно создано: {BookingId}, пользователь {UserId}, стоимость {TotalPrice} ₸", 
            booking.Id, booking.UserId, totalPrice);

        return mapper.Map<BookingDto>(booking);
    }   

    public async Task<List<BookingDto>> GetBookingsByUserAsync(Guid userId)
    {
        logger.LogInformation("Получение всех бронирований пользователя: {UserId}", userId);
        
        // Используем Where, чтобы найти все записи этого пользователя
        var bookings = await context.Bookings
            .Where(b => b.UserId == userId)
            .ToListAsync();

        logger.LogInformation("Найдено {Count} бронирований для пользователя {UserId}", bookings.Count, userId);

        return mapper.Map<List<BookingDto>>(bookings);
    }
}