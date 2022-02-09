using System.ComponentModel.DataAnnotations;

namespace Haxbot.Entities;

public class Player : Entity
{
    [Required]
    public string Name { get; set; }
    public virtual ICollection<Team> Teams { get; set; }
    public virtual ICollection<Alias> Aliases { get; set; }

    public Player(string name) : base()
    {
        Name = name;
        Teams = new HashSet<Team>();
        Aliases = new HashSet<Alias>();
    }

    public Player() : this("") { }
}
