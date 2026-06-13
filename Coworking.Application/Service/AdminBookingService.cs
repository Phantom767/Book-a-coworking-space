using AutoMapper;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Coworking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ErrorOr;

namespace Coworking.Application.Service;

public class AdminBookingService(
    IApplicationDbContext context,
    IMapper mapper,
    ILogger<AdminBookingService> logger)
    : IAdminBookingService
{
    // ── Получить все брони (с фильтрацией) ─────────────────────
    public async Task<List<BookingAdminDto>> GetAllAsync(
        BookingStatus? status = null,
        Guid? roomId = null,
        string? search = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        if (roomId.HasValue)
            query = query.Where(b => b.RoomId == roomId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(b =>
                b.User != null && b.User.Email != null && 
                (b.User.Email.ToLower().Contains(s) ||
                 b.Room.Name.ToLower().Contains(s) ||
                 (b.User.FirstName + " " + b.User.LastName).ToLower().Contains(s)));
        }

        if (dateFrom.HasValue)
            query = query.Where(b => b.StartTime >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(b => b.StartTime <= dateTo.Value);

        var bookings = await query
            .OrderByDescending(b => b.CreationTime)
            .ToListAsync();

        return bookings.Select(b => new BookingAdminDto
        {
            Id           = b.Id,
            RoomId       = b.RoomId,
            RoomName     = b.Room?.Name ?? "—",
            UserEmail    = b.User?.Email ?? "—",
            UserFullName = $"{b.User?.FirstName} {b.User?.LastName}".Trim(),
            StartTime    = b.StartTime,
            EndTime      = b.EndTime,
            TotalPrice   = b.TotalPrice,
            Status       = b.Status,
            CreatedAt    = b.CreationTime
        }).ToList();
    }

    // ── Подтвердить бронь ───────────────────────────────────────
    public async Task<ErrorOr<BookingAdminDto>> ConfirmAsync(Guid bookingId)
    {
        logger.LogInformation("Подтверждение брони: {BookingId}", bookingId);

        var booking = await context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
        {
            logger.LogWarning("Бронь не найдена: {BookingId}", bookingId);
            return Error.NotFound("Booking.NotFound", "Бронь не найдена");
        }

        if (booking.Status != BookingStatus.Pending)
            return Error.Conflict("Booking.WrongStatus",
                $"Нельзя подтвердить бронь со статусом «{booking.Status}»");

        booking.Status = BookingStatus.Confirmed;
        booking.LastModificationTime = DateTime.UtcNow;

        context.Bookings.Update(booking);
        await context.SaveChangesAsync();

        logger.LogInformation("Бронь подтверждена: {BookingId}", bookingId);

        return new BookingAdminDto
        {
            Id           = booking.Id,
            RoomName     = booking.Room?.Name ?? "—",
            UserEmail    = booking.User?.Email ?? "—",
            UserFullName = $"{booking.User?.FirstName} {booking.User?.LastName}".Trim(),
            StartTime    = booking.StartTime,
            EndTime      = booking.EndTime,
            TotalPrice   = booking.TotalPrice,
            Status       = booking.Status,
            CreatedAt    = booking.CreationTime
        };
    }

    // ── Отменить бронь ──────────────────────────────────────────
    public async Task<ErrorOr<BookingAdminDto>> CancelAsync(Guid bookingId, string? reason = null)
    {
        logger.LogInformation("Отмена брони: {BookingId}, причина: {Reason}", bookingId, reason);

        var booking = await context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
        {
            logger.LogWarning("Бронь не найдена: {BookingId}", bookingId);
            return Error.NotFound("Booking.NotFound", "Бронь не найдена");
        }

        if (booking.Status == BookingStatus.Cancelled)
            return Error.Conflict("Booking.AlreadyCancelled", "Бронь уже отменена");

        if (booking.Status == BookingStatus.Completed)
            return Error.Conflict("Booking.Completed", "Нельзя отменить завершённую бронь");

        booking.Status = BookingStatus.Cancelled;
        booking.LastModificationTime = DateTime.UtcNow;

        context.Bookings.Update(booking);
        await context.SaveChangesAsync();

        logger.LogInformation("Бронь отменена: {BookingId}", bookingId);

        return new BookingAdminDto
        {
            Id           = booking.Id,
            RoomName     = booking.Room?.Name ?? "—",
            UserEmail    = booking.User?.Email ?? "—",
            UserFullName = $"{booking.User?.FirstName} {booking.User?.LastName}".Trim(),
            StartTime    = booking.StartTime,
            EndTime      = booking.EndTime,
            TotalPrice   = booking.TotalPrice,
            Status       = booking.Status,
            CreatedAt    = booking.CreationTime
        };
    }

    // ── Статистика для дашборда ─────────────────────────────────
    public async Task<(int Total, int Pending, int Confirmed, int Cancelled)> GetStatsAsync()
    {
        var stats = await context.Bookings
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return (
            Total:     stats.Sum(s => s.Count),
            Pending:   stats.FirstOrDefault(s => s.Status == BookingStatus.Pending)?.Count   ?? 0,
            Confirmed: stats.FirstOrDefault(s => s.Status == BookingStatus.Confirmed)?.Count ?? 0,
            Cancelled: stats.FirstOrDefault(s => s.Status == BookingStatus.Cancelled)?.Count ?? 0
        );
    }
}