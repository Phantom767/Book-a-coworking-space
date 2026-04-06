using Coworking.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Room> Rooms { get; }
    DbSet<Domain.Entity.Booking> Bookings { get; }

    // Метод для сохранения изменений (стандарт для EF)
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}