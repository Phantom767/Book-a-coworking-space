using Coworking.Domain.Entity;

namespace CoworkingDomain.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id);
}