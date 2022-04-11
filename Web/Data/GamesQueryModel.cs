using Microsoft.AspNetCore.Components;

namespace Web.Data;

public class GamesQueryModel
{
    public List<string> Players { get; set; } = new();
    public bool Auth { get; set; }
    public bool Team { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool Undecided { get; set; }
    public string Stadium { get; set; } = string.Empty;
}