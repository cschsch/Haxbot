namespace Haxbot;

public class Team : Entity
{
    public virtual ICollection<Player> Players { get; set; }

    public Team() : base()
    {
        Players = new HashSet<Player>();
    }
}