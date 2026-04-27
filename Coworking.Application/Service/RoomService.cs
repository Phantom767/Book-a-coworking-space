using System.Security.Cryptography;
using AutoMapper;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Service;

public class RoomService(IApplicationDbContext context, IMapper mapper, IWebHostEnvironment env) : IRoomService
{
    public async Task<List<RoomDto>> GetAllRoomsAsync()
    {
        var rooms = await context.Rooms.ToListAsync();
        
        return mapper.Map<List<RoomDto>>(rooms);
    }

    public async Task<RoomDto> GetRoomAsync(Guid roomId)
    {
        var room =  await context.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);
        
        return mapper.Map<RoomDto>(room);
    }

    public async Task<List<RoomDto>> BookingBusyTimeAsync(Guid id, DateTime date)
    {
        var rooms = await context.Rooms.Where(x => x.Id == id)
            .Include(x => x.Bookings.Where(b => b.StartTime.Date == date.Date))
            .ToListAsync();
        
        return mapper.Map<List<RoomDto>>(rooms);
    }
    
    public async Task<string> UploadRoomPhotoAsync(Guid roomId, IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            throw new InvalidOperationException("Файл не выбран.");

        // 1. Валидация
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(photo.ContentType.ToLower()))
            throw new InvalidOperationException("Допустимые форматы: JPG, PNG, WEBP");

        if (photo.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("Файл не должен превышать 5 МБ");

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
            return room.PhotoUrl;
        }

        // 4. Удаляем старое фото физически, если оно есть и отличается
        if (!string.IsNullOrEmpty(room.PhotoUrl))
        {
            var oldPath = Path.Combine(env.WebRootPath, room.PhotoUrl.TrimStart('/'));
            if (File.Exists(oldPath))
            {
                try { File.Delete(oldPath); } catch { /* Игнорируем ошибки удаления */ }
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
        }

        var relativeUrl = $"/uploads/rooms/{fileName}";

        // 6. Обновляем сущность
        room.PhotoUrl = relativeUrl;
        room.PhotoHash = newFileHash;
        
        await UpdateAsync(room);

        return relativeUrl;
    }

    public async Task<UpdateRoomDto?> GetByIdAsync(Guid id)
    {
        var result = await context.Rooms.FirstOrDefaultAsync(x => x.Id == id);
        if (result == null) return null;
        return mapper.Map<UpdateRoomDto>(result);
    }

    public async Task UpdateAsync(UpdateRoomDto roomDto)
    {
        var existingRoom = await context.Rooms.FindAsync(roomDto.Id);
    
        if (existingRoom == null)
        {
            throw new Exception("Комната не найдена");
        }

        mapper.Map(roomDto, existingRoom);
        existingRoom.LastModificationTime = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }
}