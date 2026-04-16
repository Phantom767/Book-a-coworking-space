namespace Coworking.Domain.Entity;

public class EntityBase
{
    public Guid  Id { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastModificationTime { get; set; }
}