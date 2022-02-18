using Haxbot.Entities;
using Haxbot.Settings;
using Microsoft.EntityFrameworkCore;

namespace Haxbot;

public class HaxbotContext : DbContext
{
    public virtual DbSet<Player>? Players { get; set; }
    public virtual DbSet<Team>? Teams { get; set; }
    public virtual DbSet<Game>? Games { get; set; }
    public Configuration Configuration { get; }

    public HaxbotContext(Configuration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(Configuration.ConnectionString);
}