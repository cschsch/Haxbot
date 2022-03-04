namespace CLI;

public class QueryFilter
{
    public string[] Players { get; set; } = Array.Empty<string>();
    public bool Auth { get; set; }
    public bool Team { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public bool Undecided { get; set; }
    public string Stadium { get; set; } = string.Empty;
}