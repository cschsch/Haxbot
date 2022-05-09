using System.Collections.Immutable;

namespace Haxbot.Extensions;

public static class ImmutableHashSetExtensions
{
    public static IEnumerable<ImmutableHashSet<T>> Subsets<T>(this ImmutableHashSet<T> source)
    {
        if (!source.Any()) return Enumerable.Empty<ImmutableHashSet<T>>();
        return source.Subsets(new[] { ImmutableHashSet.Create<T>() });
    }

    private static IEnumerable<ImmutableHashSet<T>> Subsets<T>(this ImmutableHashSet<T> source, IEnumerable<ImmutableHashSet<T>> output)
    {
        if (!source.Any()) return output;

        var someElement = source.First();
        return source.Remove(someElement)
            .Subsets(output.Concat(output.Select(subset => subset.Add(someElement))));
    }
}