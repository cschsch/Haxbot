using Haxbot.Entities;
using Haxbot.Settings;
using Microsoft.EntityFrameworkCore;

namespace Haxbot;

public class HaxbotContext : DbContext
{
    public DbSet<Player>? Players { get; set; }
    public DbSet<Team>? Teams { get; set; }
    public DbSet<Game>? Games { get; set; }
    public Configuration Configuration { get; }

    public HaxbotContext(Configuration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(Configuration.ConnectionString);
}