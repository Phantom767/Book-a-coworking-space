using Coworking.Domain.Entity;
using CoworkingDomain.Entity;

namespace CoworkingDomain.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id);
}