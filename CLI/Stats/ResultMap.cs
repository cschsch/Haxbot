using CLI.Commands;

namespace CLI.Stats;

public record ResultMap<TKey>(TKey Key, GameResult Result);
