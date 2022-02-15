using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Haxbot.Entities;

[Index(nameof(Auth))]
public class Player : Entity
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Auth { get; set; }
    public virtual ICollection<Team> Teams { get; set; }

    public Player(string name, string auth) : base()
    {
        Name = name;
        Auth = auth;
        Teams = new HashSet<Team>();
    }

    public Player() : this("", "") { }
}
