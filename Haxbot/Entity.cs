using System.ComponentModel.DataAnnotations;

namespace Haxbot;

public abstract class Entity
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime Created { get; set; }

    protected Entity(Guid id, DateTime created)
    {
        Id = id;
        Created = created;
    }

    protected Entity() : this(Guid.NewGuid(), DateTime.Now) { }
}
