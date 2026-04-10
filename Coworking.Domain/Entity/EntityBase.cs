namespace Coworking.Domain.Entity;

public class EntityBase
{
    public Guid  Id { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public DateTime? LastModificationTime { get; set; }
}