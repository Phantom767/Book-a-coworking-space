using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Конфигурация цены с точностью до 2 знаков после запятой
        modelBuilder.Entity<Room>()
            .Property(r => r.PricePerHour)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalPrice)
            .HasPrecision(18, 2);

        // Индексы для быстрого поиска по времени и комнате
        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.RoomId, b.StartTime, b.EndTime });

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}