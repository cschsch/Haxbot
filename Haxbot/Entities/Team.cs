using System.ComponentModel.DataAnnotations;

namespace Haxbot.Entities;

public class Team : Entity
{
    [Required]
    public virtual ICollection<Player> Players { get; set; }

    public Team() : base()
    {
        Players = new HashSet<Player>();
    }
}