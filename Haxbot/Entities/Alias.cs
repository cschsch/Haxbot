using System.ComponentModel.DataAnnotations;

namespace Haxbot.Entities;

public class Alias : Entity
{
    [Required]
    public string Name { get; set;}
    public virtual ICollection<Player> Players { get; set; }

    public Alias(string name) : base()
    {
        Name = name;
        Players = new HashSet<Player>();
    }

    public Alias() : this("") { }
}