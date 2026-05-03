using System.Security.Cryptography;
using AutoMapper;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coworking.Application.Service;

public class RoomService(
    IApplicationDbContext context, 
    IMapper mapper, 
    IWebHostEnvironment env,
    ILogger<RoomService> logger) : IRoomService
{
    public async Task<List<RoomDto>> GetAllRoomsAsync()
    {
        logger.LogInformation("Получение всех комнат");
        
        var rooms = await context.Rooms.ToListAsync();
        
        logger.LogInformation("Найдено {Count} комнат", rooms.Count);
        
        return mapper.Map<List<RoomDto>>(rooms);
    }

    public async Task<RoomDto> GetRoomAsync(Guid roomId)
    {
        logger.LogInformation("Получение комнаты: {RoomId}", roomId);
        
        var room =  await context.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);
        
        if (room == null)
            logger.LogWarning("Комната не найдена: {RoomId}", roomId);
        
        return mapper.Map<RoomDto>(room);
    }

    public async Task<List<RoomDto>> BookingBusyTimeAsync(Guid id, DateTime date)
    {
        logger.LogInformation("Получение занятого времени для комнаты {RoomId} на дату {Date}", id, date.Date);
        
        var rooms = await context.Rooms.Where(x => x.Id == id)
            .Include(x => x.Bookings.Where(b => b.StartTime.Date == date.Date))
            .ToListAsync();
        
        return mapper.Map<List<RoomDto>>(rooms);
    }
    
    public async Task<string> UploadRoomPhotoAsync(Guid roomId, IFormFile photo)
    {
        logger.LogInformation("Загрузка фото для комнаты: {RoomId}", roomId);
        
        if (photo == null || photo.Length == 0)
        {
            logger.LogWarning("Файл фото не выбран для комнаты {RoomId}", roomId);
            throw new InvalidOperationException("Файл не выбран.");
        }

        // 1. Валидация
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(photo.ContentType.ToLower()))
        {
            logger.LogWarning("Недопустимый формат файла {ContentType} для комнаты {RoomId}", 
                photo.ContentType, roomId);
            throw new InvalidOperationException("Допустимые форматы: JPG, PNG, WEBP");
        }

        if (photo.Length > 5 * 1024 * 1024)
        {
            logger.LogWarning("Файл слишком большой {Size} для комнаты {RoomId}", photo.Length, roomId);
            throw new InvalidOperationException("Файл не должен превышать 5 МБ");
        }

        // 2. Вычисляем хеш нового файла
        string newFileHash;
        using (var md5 = MD5.Create())
        {
            await using var stream = photo.OpenReadStream();
            var hashBytes = await md5.ComputeHashAsync(stream);
            newFileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        var room = await GetByIdAsync(roomId)
            ?? throw new KeyNotFoundException($"Комната {roomId} не найдена");

        // 3. ПРОВЕРКА НА ДУБЛИКАТ
        if (!string.IsNullOrEmpty(room.PhotoHash) && room.PhotoHash == newFileHash)
        {
            logger.LogInformation("Файл идентичен существующему фото, загрузка пропущена для комнаты {RoomId}", roomId);
            return room.PhotoUrl;
        }

        // 4. Удаляем старое фото физически, если оно есть и отличается
        if (!string.IsNullOrEmpty(room.PhotoUrl))
        {
            var oldPath = Path.Combine(env.WebRootPath, room.PhotoUrl.TrimStart('/'));
            if (File.Exists(oldPath))
            {
                try 
                { 
                    File.Delete(oldPath);
                    logger.LogInformation("Старое фото удалено: {OldPath}", oldPath);
                } 
                catch (Exception ex)
                { 
                    logger.LogError(ex, "Ошибка при удалении старого фото: {OldPath}", oldPath);
                }
            }
        }

        // 5. Сохраняем новый файл
        var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var fileName = $"room_{newFileHash}{ext}"; // Имя файла теперь зависит от контента
        
        var folder = Path.Combine(env.WebRootPath, "uploads", "rooms");
        Directory.CreateDirectory(folder);
        
        var fullPath = Path.Combine(folder, fileName);
        
        if (!File.Exists(fullPath))
        {
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await photo.CopyToAsync(stream);
            logger.LogInformation("Новое фото сохранено: {FileName}", fileName);
        }

        var relativeUrl = $"/uploads/rooms/{fileName}";

        // 6. Обновляем сущность
        room.PhotoUrl = relativeUrl;
        room.PhotoHash = newFileHash;
        
        await UpdateAsync(room);
        
        logger.LogInformation("Фото для комнаты {RoomId} успешно обновлено", roomId);

        return relativeUrl;
    }

    public async Task<UpdateRoomDto?> GetByIdAsync(Guid id)
    {
        var result = await context.Rooms.FirstOrDefaultAsync(x => x.Id == id);
        if (result == null)
        {
            logger.LogWarning("Комната не найдена при получении для обновления: {RoomId}", id);
            return null;
        }
        return mapper.Map<UpdateRoomDto>(result);
    }

    public async Task UpdateAsync(UpdateRoomDto roomDto)
    {
        logger.LogInformation("Обновление комнаты: {RoomId}", roomDto.Id);
        
        var existingRoom = await context.Rooms.FindAsync(roomDto.Id);
    
        if (existingRoom == null)
        {
            logger.LogError("Комната не найдена при попытке обновления: {RoomId}", roomDto.Id);
            throw new Exception("Комната не найдена");
        }

        mapper.Map(roomDto, existingRoom);
        existingRoom.LastModificationTime = DateTime.UtcNow;

        await context.SaveChangesAsync();
        
        logger.LogInformation("Комната успешно обновлена: {RoomId}", roomDto.Id);
    }
}