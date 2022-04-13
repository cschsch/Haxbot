using System.Collections.Immutable;

namespace Web.Data;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Scan<T>(this IEnumerable<T> source, Func<T, T, T> func)
    {
        return source.Skip(1).Aggregate(
            source.Take(1).ToImmutableArray(), 
            (acc, cur) => acc.Add(func(acc.Last(), cur)));
    }

    public static IEnumerable<T> Cycle<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }

        foreach (var item in Cycle(source))
        {
            yield return item;
        }
    }
}
