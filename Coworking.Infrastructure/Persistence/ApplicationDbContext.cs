using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public new DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>().HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Конфигурация цены с точностью до 2 знаков после запятой
        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(r => r.PricePerHour)
                .HasPrecision(18, 2);

            // Настройка новых полей
            entity.Property(r => r.PhotoUrl)
                .HasMaxLength(500); // Ограничение длины ссылки

            entity.Property(r => r.PhotoHash)
                .HasMaxLength(256); // Хэш обычно фиксированной длины (SHA256 и т.д.)
        });

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